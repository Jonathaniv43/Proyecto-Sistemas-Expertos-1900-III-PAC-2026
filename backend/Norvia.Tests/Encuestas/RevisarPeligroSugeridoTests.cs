using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Encuestas;

/// <summary>
/// HU-035: confirmar o descartar los peligros sugeridos por la encuesta (RN-18). La sugerencia se arma directo en
/// la base, como la deja HU-033, para no depender de las respuestas.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class RevisarPeligroSugeridoTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Confirmar_deja_el_riesgo_confirmado_con_su_actividad_y_quien_lo_reviso()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var s = await CrearSugerenciaAsync(empresa);

        var r = await cliente.PatchAsJsonAsync(Ruta(s.EncuestaId, s.RiesgoId),
            new { estado = "confirmado", actividad = "  Carga de camiones  " });

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal("confirmado", json.GetProperty("estado").GetString());
        Assert.Equal("Carga de camiones", json.GetProperty("actividad").GetString());

        var riesgo = await LeerRiesgoAsync(empresa.EmpresaId, s.RiesgoId);
        Assert.Equal(EstadoRiesgo.Confirmado, riesgo.Estado);
        Assert.Equal("Carga de camiones", riesgo.Actividad);
        Assert.NotNull(riesgo.RevisadoPorId);
        Assert.NotNull(riesgo.RevisadoEn);
    }

    [Fact]
    public async Task Descartar_deja_el_riesgo_descartado_fuera_de_la_matriz()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var s = await CrearSugerenciaAsync(empresa);

        var r = await cliente.PatchAsJsonAsync(Ruta(s.EncuestaId, s.RiesgoId), new { estado = "descartado" });

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var riesgo = await LeerRiesgoAsync(empresa.EmpresaId, s.RiesgoId);
        Assert.Equal(EstadoRiesgo.Descartado, riesgo.Estado);
        Assert.NotNull(riesgo.RevisadoEn);
    }

    [Fact]
    public async Task Una_sugerencia_ya_revisada_no_se_revisa_de_nuevo()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var s = await CrearSugerenciaAsync(empresa);
        await cliente.PatchAsJsonAsync(Ruta(s.EncuestaId, s.RiesgoId), new { estado = "descartado" });

        var r = await cliente.PatchAsJsonAsync(Ruta(s.EncuestaId, s.RiesgoId), new { estado = "confirmado" });

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal(EstadoRiesgo.Descartado, (await LeerRiesgoAsync(empresa.EmpresaId, s.RiesgoId)).Estado);
    }

    [Theory]
    [InlineData("sugerido", null)]
    [InlineData("descartado", "Bodega")]
    public async Task Rechaza_decisiones_invalidas(string estado, string? actividad)
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var s = await CrearSugerenciaAsync(empresa);

        var r = await cliente.PatchAsJsonAsync(Ruta(s.EncuestaId, s.RiesgoId), new { estado, actividad });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal(EstadoRiesgo.Sugerido, (await LeerRiesgoAsync(empresa.EmpresaId, s.RiesgoId)).Estado);
    }

    [Fact]
    public async Task La_sugerencia_debe_ser_de_la_encuesta_indicada()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var s = await CrearSugerenciaAsync(empresa);
        var otra = await CrearSugerenciaAsync(empresa);

        var r = await cliente.PatchAsJsonAsync(Ruta(otra.EncuestaId, s.RiesgoId), new { estado = "confirmado" });

        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Otra_empresa_no_puede_revisar_una_sugerencia_ajena()
    {
        var (a, _) = await EmpresaConResponsableAsync();
        var (_, clienteB) = await EmpresaConResponsableAsync();
        var s = await CrearSugerenciaAsync(a);

        var r = await clienteB.PatchAsJsonAsync(Ruta(s.EncuestaId, s.RiesgoId), new { estado = "confirmado" });

        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal(EstadoRiesgo.Sugerido, (await LeerRiesgoAsync(a.EmpresaId, s.RiesgoId)).Estado);
    }

    [Fact]
    public async Task Un_colaborador_no_tiene_acceso()
    {
        var empresa = await _escenario.CrearEmpresaAsync("A", 1);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var s = await CrearSugerenciaAsync(empresa);

        var r = await colaborador.PatchAsJsonAsync(Ruta(s.EncuestaId, s.RiesgoId), new { estado = "confirmado" });

        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    // ---------- Apoyo ----------

    private static string Ruta(Guid encuestaId, Guid riesgoId) => $"/api/encuestas/{encuestaId}/peligros-sugeridos/{riesgoId}";

    private sealed record Sugerencia(Guid EncuestaId, Guid RiesgoId);

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("A", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    /// <summary>Encuesta publicada con un peligro sugerido, tal como lo registra HU-033.</summary>
    private async Task<Sugerencia> CrearSugerenciaAsync(EmpresaDePrueba empresa)
    {
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var peligro = new Peligro
        {
            Codigo = $"p{Guid.NewGuid():N}"[..20], Nombre = $"Peligro {Guid.NewGuid():N}"[..20],
            Clasificacion = ClasificacionPeligro.Ergonomico
        };
        var encuesta = new Encuesta { Titulo = "Diagnóstico", Estado = EstadoEncuesta.Publicada, CreadaPorId = empresa.AdminVinculoId };
        db.AddRange(peligro, encuesta);
        var riesgo = new RiesgoEmpresa { PeligroId = peligro.Id, EncuestaId = encuesta.Id, Estado = EstadoRiesgo.Sugerido };
        db.RiesgoEmpresa.Add(riesgo);
        await db.SaveChangesAsync();
        return new Sugerencia(encuesta.Id, riesgo.Id);
    }

    private async Task<RiesgoEmpresa> LeerRiesgoAsync(Guid empresaId, Guid riesgoId)
    {
        await using var db = api.NuevoContexto(empresaId);
        return await db.RiesgoEmpresa.AsNoTracking().SingleAsync(r => r.Id == riesgoId);
    }
}
