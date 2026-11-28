using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Documentos;

/// <summary>
/// HU-072: aprobar el borrador lo deja vigente con quién y cuándo (RN-50), con una sola vigente por documento (RN-51);
/// aprobar una política avisa a cada vínculo activo de la empresa (RN-75, HU-116).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class AprobarDocumentoTests(NorviaApiFactory api)
{
    private const string SinFaltantes = "# Política de {{RazonSocial}}\n\nLa empresa cumple el reglamento.";
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Aprobar_una_politica_la_deja_vigente_y_avisa_a_cada_vinculo_activo()
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync(TipoDocumento.Politica, SinFaltantes);
        var colaborador = await Escenario.AgregarColaboradorAsync(empresa, $"col.{Guid.NewGuid():N}@pruebas.hn");
        var suspendido = await Escenario.AgregarColaboradorAsync(empresa, $"sus.{Guid.NewGuid():N}@pruebas.hn");
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            (await db.VinculoEmpresa.SingleAsync(v => v.Id == suspendido)).Estado = EstadoVinculo.Suspendido;
            await db.SaveChangesAsync();
        }
        var id = await GenerarAsync(responsable, plantilla, "Política SST 2026");

        var r = await responsable.PostAsync($"/api/documentos/{id}/aprobacion", null);

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var version = (await Escenario.Json(r)).GetProperty("version");
        Assert.Equal("vigente", version.GetProperty("estado").GetString());
        Assert.Equal(1, version.GetProperty("numeroVersion").GetInt32());
        Assert.Equal("Responsable Sst", version.GetProperty("aprobadaPorNombre").GetString());
        Assert.NotEqual(System.Text.Json.JsonValueKind.Null, version.GetProperty("aprobadaEn").ValueKind);

        await using var verificacion = api.NuevoContexto(empresa.EmpresaId);
        var versionId = version.GetProperty("id").GetGuid();
        var avisos = await verificacion.Notificacion.Where(n => n.VersionDocumentoId == versionId).ToListAsync();
        var activos = await verificacion.VinculoEmpresa.Where(v => v.Estado == EstadoVinculo.Activo).Select(v => v.Id).ToListAsync();
        Assert.Equal(activos.Order(), avisos.Select(n => n.DestinatarioId).Order());
        Assert.Contains(colaborador, avisos.Select(n => n.DestinatarioId));
        Assert.DoesNotContain(suspendido, avisos.Select(n => n.DestinatarioId));
        Assert.All(avisos, n =>
        {
            Assert.Equal(TipoNotificacion.PoliticaPublicada, n.Tipo);
            Assert.Equal("Nueva política: Política SST 2026", n.Titulo);
            // La API no envía dentro de la petición: deja el correo pendiente y avisa al proceso de fondo (RN-78).
            Assert.Equal(EstadoCorreo.Pendiente, n.CorreoEstado);
        });

        // Ya no se edita (RN-50) ni se aprueba otra vez.
        var editar = await responsable.PutAsJsonAsync($"/api/documentos/{id}/borrador", new { contenido = "# Cambiado" });
        var otraVez = await responsable.PostAsync($"/api/documentos/{id}/aprobacion", null);
        Assert.Equal(HttpStatusCode.Conflict, editar.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, otraVez.StatusCode);
        Assert.Equal("documentos.sin_borrador", (await Escenario.Json(otraVez)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Un_procedimiento_se_aprueba_sin_avisos()
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync(TipoDocumento.Procedimiento, SinFaltantes);
        var id = await GenerarAsync(responsable, plantilla);

        var r = await responsable.PostAsync($"/api/documentos/{id}/aprobacion", null);

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var versionId = (await Escenario.Json(r)).GetProperty("version").GetProperty("id").GetGuid();
        Assert.False(await db.Notificacion.AnyAsync(n => n.VersionDocumentoId == versionId));
    }

    [Fact]
    public async Task No_se_aprueba_con_datos_por_completar()
    {
        var (empresa, responsable, plantilla) =
            await EmpresaConPlantillaAsync(TipoDocumento.Politica, "# Política\n\nAprueba: [Completar: gerente]");
        var id = await GenerarAsync(responsable, plantilla);

        var r = await responsable.PostAsync($"/api/documentos/{id}/aprobacion", null);

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("documentos.por_completar", (await Escenario.Json(r)).GetProperty("codigo").GetString());
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(EstadoVersionDocumento.Borrador, (await db.VersionDocumento.SingleAsync(v => v.DocumentoId == id)).Estado);
        Assert.False(await db.Notificacion.AnyAsync(n => n.VersionDocumento!.DocumentoId == id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task La_vigente_anterior_pasa_a_reemplazada(bool borradorConIdMenor)
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync(TipoDocumento.Procedimiento, SinFaltantes);
        var id = await GenerarAsync(responsable, plantilla);
        Guid v1, v2;
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            // Versión nueva (HU-073 todavía no existe): la 1 aprobada y la 2 en borrador. El orden de los id cambia el
            // orden en que EF manda los UPDATE: en los dos casos la vigente única (RN-51) se respeta.
            var primera = await db.VersionDocumento.SingleAsync(v => v.DocumentoId == id);
            primera.Estado = EstadoVersionDocumento.Vigente;
            primera.AprobadaPorId = empresa.AdminVinculoId;
            primera.AprobadaEn = DateTime.UtcNow;
            var segunda = new VersionDocumento
            {
                DocumentoId = id, NumeroVersion = 2, Contenido = "# Versión 2", CreadaPorId = empresa.AdminVinculoId
            };
            if (borradorConIdMenor) segunda.Id = Guid.Parse("00000000-0000-7000-8000-000000000001");
            db.VersionDocumento.Add(segunda);
            await db.SaveChangesAsync();
            (v1, v2) = (primera.Id, segunda.Id);
        }

        var r = await responsable.PostAsync($"/api/documentos/{id}/aprobacion", null);

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(2, (await Escenario.Json(r)).GetProperty("version").GetProperty("numeroVersion").GetInt32());
        await using var verificacion = api.NuevoContexto(empresa.EmpresaId);
        var estados = await verificacion.VersionDocumento.Where(v => v.DocumentoId == id).ToDictionaryAsync(v => v.Id, v => v.Estado);
        Assert.Equal(EstadoVersionDocumento.Reemplazada, estados[v1]);
        Assert.Equal(EstadoVersionDocumento.Vigente, estados[v2]);
    }

    [Fact]
    public async Task Solo_el_responsable_de_la_empresa_aprueba()
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync(TipoDocumento.Politica, SinFaltantes);
        var id = await GenerarAsync(responsable, plantilla);
        var (_, otraEmpresa, _) = await EmpresaConPlantillaAsync(TipoDocumento.Politica, SinFaltantes);

        var admin = await empresa.Cliente.PostAsync($"/api/documentos/{id}/aprobacion", null);
        var ajena = await otraEmpresa.PostAsync($"/api/documentos/{id}/aprobacion", null);
        var inexistente = await responsable.PostAsync($"/api/documentos/{Guid.NewGuid()}/aprobacion", null);

        Assert.Equal(HttpStatusCode.Forbidden, admin.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, ajena.StatusCode);
        Assert.Equal("documentos.no_encontrado", (await Escenario.Json(ajena)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(EstadoVersionDocumento.Borrador, (await db.VersionDocumento.SingleAsync(v => v.DocumentoId == id)).Estado);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable, Guid Plantilla)> EmpresaConPlantillaAsync(
        TipoDocumento tipo, string contenido)
    {
        var empresa = await _escenario.CrearEmpresaAsync($"Apro{Guid.NewGuid():N}"[..12], rubroId: 1);
        var responsable = await _escenario.ClienteResponsableSstAsync(empresa, $"sst.{Guid.NewGuid():N}@pruebas.hn");
        var plantilla = new PlantillaDocumento
        {
            Codigo = $"a_{Guid.NewGuid():N}"[..20], Titulo = "Plantilla para aprobar", TipoDocumento = tipo, Contenido = contenido
        };
        await using (var db = api.NuevoContexto(empresaId: null))
        {
            db.PlantillaDocumento.Add(plantilla);
            await db.SaveChangesAsync();
        }
        return (empresa, responsable, plantilla.Id);
    }

    private static async Task<Guid> GenerarAsync(HttpClient cliente, Guid plantilla, string? titulo = null)
    {
        var r = await cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla, titulo });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }
}
