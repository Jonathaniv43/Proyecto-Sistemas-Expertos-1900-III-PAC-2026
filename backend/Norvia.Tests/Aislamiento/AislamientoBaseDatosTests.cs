using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;
using Npgsql;

namespace Norvia.Tests.Aislamiento;

/// <summary>
/// HU-098 en la capa de datos: el filtro global oculta lo ajeno (RN-05), el interceptor fija y protege
/// EmpresaId (RN-06) y las FK compuestas impiden mezclar empresas aunque se salte la aplicación (RN-07, D15).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class AislamientoBaseDatosTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task El_filtro_global_solo_devuelve_filas_de_la_empresa_de_la_sesion()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        await using var dbA = api.NuevoContexto(a.EmpresaId);
        var visibles = await dbA.VinculoEmpresa.Select(v => v.EmpresaId).Distinct().ToListAsync();
        Assert.Equal([a.EmpresaId], visibles);
        Assert.Null(await dbA.VinculoEmpresa.FirstOrDefaultAsync(v => v.Id == b.AdminVinculoId));
    }

    [Fact]
    public async Task Sin_sesion_de_empresa_las_tablas_de_empresa_no_devuelven_filas()
    {
        await _escenario.CrearEmpresaAsync("A");

        await using var db = api.NuevoContexto(empresaId: null);
        Assert.Equal(0, await db.VinculoEmpresa.CountAsync());
    }

    [Fact]
    public async Task Al_insertar_se_usa_la_empresa_de_la_sesion_aunque_la_fila_traiga_otra()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        await using var dbA = api.NuevoContexto(a.EmpresaId);
        var historial = new HistorialEstadoVinculo
        {
            EmpresaId = b.EmpresaId, // intento de escribir en B
            VinculoEmpresaId = a.AdminVinculoId,
            EstadoNuevo = EstadoVinculo.Activo,
            MotivoCodigo = MotivoEstadoVinculo.DecisionAdmin,
            RegistradoPorId = a.AdminVinculoId
        };
        dbA.HistorialEstadoVinculo.Add(historial);
        await dbA.SaveChangesAsync();

        Assert.Equal(a.EmpresaId, historial.EmpresaId);
    }

    [Fact]
    public async Task No_se_puede_cambiar_la_empresa_de_un_registro_existente()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        await using var dbA = api.NuevoContexto(a.EmpresaId);
        var vinculo = await dbA.VinculoEmpresa.SingleAsync(v => v.Id == a.AdminVinculoId);
        vinculo.EmpresaId = b.EmpresaId;

        await Assert.ThrowsAsync<InvalidOperationException>(() => dbA.SaveChangesAsync());
    }

    [Fact]
    public async Task La_base_rechaza_un_registro_que_relaciona_datos_de_dos_empresas()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        // Historial de A que apunta a un vínculo de B. Se escribe en nombre de A (sin pasar por la API);
        // lo detiene la FK compuesta (EmpresaId, VinculoEmpresaId) → VinculoEmpresa(EmpresaId, Id).
        await using var dbA = api.NuevoContexto(a.EmpresaId);
        dbA.HistorialEstadoVinculo.Add(new HistorialEstadoVinculo
        {
            VinculoEmpresaId = b.AdminVinculoId,
            EstadoNuevo = EstadoVinculo.Suspendido,
            MotivoCodigo = MotivoEstadoVinculo.Otro,
            MotivoDetalle = "intento de mezclar empresas",
            RegistradoPorId = a.AdminVinculoId
        });

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => dbA.SaveChangesAsync());
        var pg = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, pg.SqlState);
    }

    [Fact]
    public async Task La_base_rechaza_una_encuesta_con_creador_de_otra_empresa()
    {
        var a = await _escenario.CrearEmpresaAsync("A");
        var b = await _escenario.CrearEmpresaAsync("B");

        await using var dbA = api.NuevoContexto(a.EmpresaId);
        dbA.Encuesta.Add(new Encuesta
        {
            Titulo = "Cruce ilegal",
            CreadaPorId = b.AdminVinculoId
        });

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => dbA.SaveChangesAsync());
        var pg = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, pg.SqlState);
    }

    [Fact]
    public async Task El_filtro_global_y_fk_compuesta_protegen_accidente_entre_empresas()
    {
        var a = await _escenario.CrearEmpresaAsync("AccA");
        var b = await _escenario.CrearEmpresaAsync("AccB");

        Guid eventoAId;
        await using (var dbA = api.NuevoContexto(a.EmpresaId))
        {
            var evento = new Accidente
            {
                EmpresaId = b.EmpresaId, // interceptor debe sobrescribir con a.EmpresaId
                Tipo = TipoAccidente.CondicionInsegura,
                FechaHoraEvento = DateTime.UtcNow.AddMinutes(-5),
                Descripcion = "Prueba aislamiento BD"
            };
            dbA.Accidente.Add(evento);
            await dbA.SaveChangesAsync();
            Assert.Equal(a.EmpresaId, evento.EmpresaId);
            eventoAId = evento.Id;
        }

        await using (var dbB = api.NuevoContexto(b.EmpresaId))
            Assert.Null(await dbB.Accidente.FirstOrDefaultAsync(x => x.Id == eventoAId));

        await using (var dbSinSesion = api.NuevoContexto(empresaId: null))
            Assert.Equal(0, await dbSinSesion.Accidente.CountAsync());

        // FK compuesta (EmpresaId, DescartadoPorId) rechaza un vínculo de otra empresa (HU-049 / D15).
        await using var dbCruce = api.NuevoContexto(a.EmpresaId);
        dbCruce.Accidente.Add(new Accidente
        {
            Tipo = TipoAccidente.Incidente,
            FechaHoraEvento = DateTime.UtcNow.AddMinutes(-5),
            Descripcion = "Cruce ilegal de descartador",
            Estado = EstadoAccidente.Descartado,
            DescartadoPorId = b.AdminVinculoId,
            DescartadoEn = DateTime.UtcNow
        });

        var error = await Assert.ThrowsAsync<DbUpdateException>(() => dbCruce.SaveChangesAsync());
        var pg = Assert.IsType<PostgresException>(error.InnerException);
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, pg.SqlState);
    }
}
