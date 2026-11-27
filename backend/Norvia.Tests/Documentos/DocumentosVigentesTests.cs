using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Documentos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Documentos;

/// <summary>
/// HU-074: cualquier persona de la empresa consulta las versiones vigentes; nunca los borradores ni las reemplazadas.
/// HU-116: la notificación de una política lleva el documento para abrir su versión vigente.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class DocumentosVigentesTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Todas_las_personas_ven_solo_las_versiones_vigentes_y_buscan_por_titulo()
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync();
        var politica = await GenerarAsync(responsable, plantilla, "Política SST 2026");
        var procedimiento = await GenerarAsync(responsable, plantilla, "Procedimiento de investigación");
        var borrador = await GenerarAsync(responsable, plantilla, "Política en revisión");
        await AprobarAsync(responsable, politica);
        await AprobarAsync(responsable, procedimiento);
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            // El procedimiento cambia de tipo para probar el orden; y su v2 aprobada reemplaza a la v1 (HU-073 no existe).
            (await db.Documento.SingleAsync(d => d.Id == procedimiento)).TipoDocumento = TipoDocumento.Procedimiento;
            var v1 = await db.VersionDocumento.SingleAsync(v => v.DocumentoId == procedimiento);
            v1.Estado = EstadoVersionDocumento.Reemplazada;
            await db.SaveChangesAsync();
            db.VersionDocumento.Add(new VersionDocumento
            {
                DocumentoId = procedimiento, NumeroVersion = 2, Contenido = "# Versión 2", Estado = EstadoVersionDocumento.Vigente,
                CreadaPorId = empresa.AdminVinculoId, AprobadaPorId = empresa.AdminVinculoId, AprobadaEn = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        var (_, otraResponsable, otraPlantilla) = await EmpresaConPlantillaAsync();
        await AprobarAsync(otraResponsable, await GenerarAsync(otraResponsable, otraPlantilla, "Política ajena"));

        foreach (var rol in new[] { "colaborador", "encuestado" })
        {
            var cliente = await _escenario.ClienteConRolAsync(empresa, rol);

            var todos = await Escenario.Json(await cliente.GetAsync("/api/documentos/vigentes"));
            var filas = todos.EnumerateArray().ToList();
            Assert.Equal(new[] { "Política SST 2026", "Procedimiento de investigación" },
                filas.Select(f => f.GetProperty("titulo").GetString()).ToArray());
            Assert.Equal(new[] { "politica", "procedimiento" }, filas.Select(f => f.GetProperty("tipoDocumento").GetString()).ToArray());
            Assert.Equal(2, filas[1].GetProperty("numeroVersion").GetInt32());

            var buscados = await Escenario.Json(await cliente.GetAsync("/api/documentos/vigentes?buscar=%20POLÍTICA%20"));
            Assert.Equal(politica, Assert.Single(buscados.EnumerateArray().ToList()).GetProperty("id").GetGuid());

            // El contenido de la vigente, sin datos del borrador ni de la plantilla.
            var r = await cliente.GetAsync($"/api/documentos/vigentes/{procedimiento}");
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);
            var detalle = await Escenario.Json(r);
            Assert.Equal("# Versión 2", detalle.GetProperty("contenido").GetString());
            Assert.Equal(2, detalle.GetProperty("numeroVersion").GetInt32());
            Assert.False(detalle.TryGetProperty("plantillaTitulo", out _));

            // El borrador no se publica; el documento de otra empresa tampoco existe aquí.
            Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/documentos/vigentes/{borrador}")).StatusCode);
            // Pero el colaborador no ve los borradores ni la lista de trabajo.
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync($"/api/documentos/{borrador}")).StatusCode);
        }
    }

    [Fact]
    public async Task La_busqueda_no_acepta_mas_de_100_caracteres()
    {
        var (_, responsable, _) = await EmpresaConPlantillaAsync();

        var r = await responsable.GetAsync($"/api/documentos/vigentes?buscar={new string('a', 101)}");

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task La_notificacion_de_una_politica_lleva_el_documento()
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync();
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var id = await GenerarAsync(responsable, plantilla, "Política SST 2026");
        await AprobarAsync(responsable, id);

        var bandeja = await Escenario.Json(await colaborador.GetAsync("/api/notificaciones"));

        var aviso = Assert.Single(bandeja.GetProperty("notificaciones").EnumerateArray().ToList());
        Assert.Equal("politica_publicada", aviso.GetProperty("tipo").GetString());
        Assert.Equal(id, aviso.GetProperty("documentoId").GetGuid());
        Assert.Equal(HttpStatusCode.OK, (await colaborador.GetAsync($"/api/documentos/vigentes/{id}")).StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable, Guid Plantilla)> EmpresaConPlantillaAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync($"Vig{Guid.NewGuid():N}"[..12], rubroId: 1);
        var responsable = await _escenario.ClienteResponsableSstAsync(empresa, $"sst.{Guid.NewGuid():N}@pruebas.hn");
        var plantilla = new PlantillaDocumento
        {
            Codigo = $"v_{Guid.NewGuid():N}"[..20], Titulo = "Plantilla vigente", TipoDocumento = TipoDocumento.Politica,
            Contenido = "# Política de {{RazonSocial}}\n\nLa empresa cumple el reglamento."
        };
        await using (var db = api.NuevoContexto(empresaId: null))
        {
            db.PlantillaDocumento.Add(plantilla);
            await db.SaveChangesAsync();
        }
        return (empresa, responsable, plantilla.Id);
    }

    private static async Task<Guid> GenerarAsync(HttpClient cliente, Guid plantilla, string titulo)
    {
        var r = await cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla, titulo });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task AprobarAsync(HttpClient cliente, Guid id) =>
        (await cliente.PostAsync($"/api/documentos/{id}/aprobacion", null)).EnsureSuccessStatusCode();
}
