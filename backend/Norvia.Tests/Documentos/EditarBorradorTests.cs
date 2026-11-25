using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Documentos;

/// <summary>HU-071: editar el borrador de un documento sin alterar la plantilla; solo el borrador (RN-50).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class EditarBorradorTests(NorviaApiFactory api)
{
    private const string ContenidoPlantilla = "# Política de {{RazonSocial}}\n\nEn [Completar: dirección].";
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Guarda_el_contenido_sin_tocar_la_plantilla_y_recalcula_lo_por_completar()
    {
        var (_, cliente, plantilla) = await EmpresaConDocumentoAsync();
        var id = await GenerarAsync(cliente, plantilla);
        const string nuevo = "# Política\r\n\r\nEn Col. Palmira, Tegucigalpa. {{RazonSocial}} no se reemplaza.\r\n\r\n**Aprueba:** [Completar: gerente]\r\n\r\n";

        var r = await cliente.PutAsJsonAsync($"/api/documentos/{id}/borrador", new { contenido = nuevo });

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Escenario.Json(r);
        var version = json.GetProperty("version");
        Assert.Equal("# Política\n\nEn Col. Palmira, Tegucigalpa. {{RazonSocial}} no se reemplaza.\n\n**Aprueba:** [Completar: gerente]",
            version.GetProperty("contenido").GetString());
        Assert.Equal("borrador", version.GetProperty("estado").GetString());
        Assert.Equal(1, version.GetProperty("numeroVersion").GetInt32());
        Assert.Equal(new[] { "gerente" }, json.GetProperty("porCompletar").EnumerateArray().Select(x => x.GetString()).ToArray());

        // Se ve igual al volver a abrirlo, y la plantilla no cambió.
        var detalle = await Escenario.Json(await cliente.GetAsync($"/api/documentos/{id}"));
        Assert.Contains("Col. Palmira", detalle.GetProperty("version").GetProperty("contenido").GetString());
        await using var db = api.NuevoContexto(empresaId: null);
        Assert.Equal(ContenidoPlantilla, (await db.PlantillaDocumento.SingleAsync(p => p.Id == plantilla)).Contenido);
    }

    [Fact]
    public async Task No_acepta_un_documento_vacio_ni_demasiado_largo()
    {
        var (_, cliente, plantilla) = await EmpresaConDocumentoAsync();
        var id = await GenerarAsync(cliente, plantilla);

        var vacio = await cliente.PutAsJsonAsync($"/api/documentos/{id}/borrador", new { contenido = "  \n " });
        var largo = await cliente.PutAsJsonAsync($"/api/documentos/{id}/borrador", new { contenido = new string('a', 50_001) });

        Assert.Equal(HttpStatusCode.BadRequest, vacio.StatusCode);
        Assert.True((await Escenario.Json(vacio)).GetProperty("errors").TryGetProperty("Contenido", out _));
        Assert.Equal(HttpStatusCode.BadRequest, largo.StatusCode);
    }

    [Fact]
    public async Task Una_version_aprobada_no_se_edita()
    {
        var (empresa, cliente, plantilla) = await EmpresaConDocumentoAsync();
        var id = await GenerarAsync(cliente, plantilla);
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            // Aprobada (HU-072 todavía no existe): vigente, con quién y cuándo.
            var v = await db.VersionDocumento.SingleAsync(x => x.DocumentoId == id);
            v.Estado = EstadoVersionDocumento.Vigente;
            v.AprobadaPorId = empresa.AdminVinculoId;
            v.AprobadaEn = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var r = await cliente.PutAsJsonAsync($"/api/documentos/{id}/borrador", new { contenido = "# Cambiado" });

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("documentos.sin_borrador", (await Escenario.Json(r)).GetProperty("codigo").GetString());
        await using var verificacion = api.NuevoContexto(empresa.EmpresaId);
        Assert.NotEqual("# Cambiado", (await verificacion.VersionDocumento.SingleAsync(x => x.DocumentoId == id)).Contenido);
    }

    [Fact]
    public async Task Solo_el_responsable_de_la_empresa_edita()
    {
        var (empresa, cliente, plantilla) = await EmpresaConDocumentoAsync();
        var id = await GenerarAsync(cliente, plantilla);
        var (_, otraEmpresa, _) = await EmpresaConDocumentoAsync();

        var admin = await empresa.Cliente.PutAsJsonAsync($"/api/documentos/{id}/borrador", new { contenido = "# Admin" });
        var ajena = await otraEmpresa.PutAsJsonAsync($"/api/documentos/{id}/borrador", new { contenido = "# Ajena" });
        var inexistente = await cliente.PutAsJsonAsync($"/api/documentos/{Guid.NewGuid()}/borrador", new { contenido = "# Nada" });

        Assert.Equal(HttpStatusCode.Forbidden, admin.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, ajena.StatusCode);
        Assert.Equal("documentos.no_encontrado", (await Escenario.Json(ajena)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable, Guid Plantilla)> EmpresaConDocumentoAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync($"Edit{Guid.NewGuid():N}"[..12], rubroId: 1);
        var responsable = await _escenario.ClienteResponsableSstAsync(empresa, $"sst.{Guid.NewGuid():N}@pruebas.hn");
        var plantilla = new PlantillaDocumento
        {
            Codigo = $"e_{Guid.NewGuid():N}"[..20], Titulo = "Plantilla para editar", TipoDocumento = TipoDocumento.Politica,
            Contenido = ContenidoPlantilla
        };
        await using (var db = api.NuevoContexto(empresaId: null))
        {
            db.PlantillaDocumento.Add(plantilla);
            await db.SaveChangesAsync();
        }
        return (empresa, responsable, plantilla.Id);
    }

    private static async Task<Guid> GenerarAsync(HttpClient cliente, Guid plantilla)
    {
        var r = await cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }
}
