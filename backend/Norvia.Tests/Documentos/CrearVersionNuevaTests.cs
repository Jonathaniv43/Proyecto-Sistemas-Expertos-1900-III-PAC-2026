using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Documentos;

/// <summary>
/// HU-073: crear una versión nueva a partir de la vigente (queda en borrador) y consultar versiones anteriores
/// sin alterar la vigente ni perder el historial (RN-50, RN-51, D13).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class CrearVersionNuevaTests(NorviaApiFactory api)
{
    private const string ContenidoInicial = "# Política de {{RazonSocial}}\n\nVersión aprobada inicial.";
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Crear_version_nueva_copia_la_vigente_como_borrador_sin_afectar_la_publicada()
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync(TipoDocumento.Politica, ContenidoInicial);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var id = await GenerarYAprobarAsync(responsable, plantilla, "Política SST 2026");

        var vigenteAntes = await Escenario.Json(await colaborador.GetAsync($"/api/documentos/vigentes/{id}"));
        var contenidoV1 = vigenteAntes.GetProperty("contenido").GetString()!;

        var r = await responsable.PostAsync($"/api/documentos/{id}/versiones", null);

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var creada = await Escenario.Json(r);
        var v2 = creada.GetProperty("version");
        Assert.Equal(2, v2.GetProperty("numeroVersion").GetInt32());
        Assert.Equal("borrador", v2.GetProperty("estado").GetString());
        Assert.Equal(contenidoV1, v2.GetProperty("contenido").GetString());
        Assert.Equal("Responsable Sst", v2.GetProperty("creadaPorNombre").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, v2.GetProperty("aprobadaEn").ValueKind);
        Assert.Empty(creada.GetProperty("porCompletar").EnumerateArray());

        // La versión vigente que consultan las personas de la empresa sigue siendo la v1 (HU-074).
        var vigenteDespues = await Escenario.Json(await colaborador.GetAsync($"/api/documentos/vigentes/{id}"));
        Assert.Equal(1, vigenteDespues.GetProperty("numeroVersion").GetInt32());
        Assert.Equal(contenidoV1, vigenteDespues.GetProperty("contenido").GetString());
    }

    [Fact]
    public async Task Las_versiones_anteriores_siguen_disponibles_en_historial_y_detalle_tras_aprobar_cambios()
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync(TipoDocumento.Procedimiento, ContenidoInicial);
        var id = await GenerarYAprobarAsync(responsable, plantilla, "Procedimiento de emergencia");

        // Crear v2, editarla y aprobarla → v1 pasa a reemplazada y v2 queda vigente.
        var crearV2 = await responsable.PostAsync($"/api/documentos/{id}/versiones", null);
        Assert.Equal(HttpStatusCode.Created, crearV2.StatusCode);

        var editarV2 = await responsable.PutAsJsonAsync($"/api/documentos/{id}/borrador",
            new { contenido = "# Procedimiento v2\n\nActualizado con nuevos puntos de reunión." });
        Assert.Equal(HttpStatusCode.OK, editarV2.StatusCode);

        var aprobarV2 = await responsable.PostAsync($"/api/documentos/{id}/aprobacion", null);
        Assert.Equal(HttpStatusCode.OK, aprobarV2.StatusCode);

        // Crear v3 (borrador) a partir de v2 vigente.
        var crearV3 = await responsable.PostAsync($"/api/documentos/{id}/versiones", null);
        Assert.Equal(HttpStatusCode.Created, crearV3.StatusCode);

        // Historial completo: v3 borrador, v2 vigente, v1 reemplazada (orden descendente).
        var listar = await responsable.GetAsync($"/api/documentos/{id}/versiones");
        Assert.Equal(HttpStatusCode.OK, listar.StatusCode);
        var versiones = (await Escenario.Json(listar)).EnumerateArray().ToList();
        Assert.Equal(3, versiones.Count);
        Assert.Equal([3, 2, 1], versiones.Select(v => v.GetProperty("numeroVersion").GetInt32()));
        Assert.Equal(["borrador", "vigente", "reemplazada"], versiones.Select(v => v.GetProperty("estado").GetString()));

        // Consultar el contenido íntegro de la v1 reemplazada y de la v2 vigente.
        var v1Id = versiones[2].GetProperty("id").GetGuid();
        var v2Id = versiones[1].GetProperty("id").GetGuid();

        var consultaV1 = await responsable.GetAsync($"/api/documentos/{id}/versiones/{v1Id}");
        Assert.Equal(HttpStatusCode.OK, consultaV1.StatusCode);
        var jsonV1 = await Escenario.Json(consultaV1);
        Assert.Equal(1, jsonV1.GetProperty("version").GetProperty("numeroVersion").GetInt32());
        Assert.Equal("reemplazada", jsonV1.GetProperty("version").GetProperty("estado").GetString());
        Assert.Contains("Versión aprobada inicial.", jsonV1.GetProperty("version").GetProperty("contenido").GetString());

        var consultaV2 = await empresa.Cliente.GetAsync($"/api/documentos/{id}/versiones/{v2Id}");
        Assert.Equal(HttpStatusCode.OK, consultaV2.StatusCode);
        var jsonV2 = await Escenario.Json(consultaV2);
        Assert.Equal(2, jsonV2.GetProperty("version").GetProperty("numeroVersion").GetInt32());
        Assert.Equal("vigente", jsonV2.GetProperty("version").GetProperty("estado").GetString());
        Assert.Contains("nuevos puntos de reunión", jsonV2.GetProperty("version").GetProperty("contenido").GetString());
    }

    [Fact]
    public async Task No_se_puede_crear_version_nueva_si_ya_existe_un_borrador()
    {
        var (_, responsable, plantilla) = await EmpresaConPlantillaAsync(TipoDocumento.Politica, ContenidoInicial);

        // Caso 1: v1 todavía en borrador.
        var idBorrador = await GenerarAsync(responsable, plantilla, "Solo borrador");
        var r1 = await responsable.PostAsync($"/api/documentos/{idBorrador}/versiones", null);
        Assert.Equal(HttpStatusCode.Conflict, r1.StatusCode);
        Assert.Equal("documentos.ya_tiene_borrador", (await Escenario.Json(r1)).GetProperty("codigo").GetString());

        // Caso 2: v1 vigente + v2 borrador abierto.
        var idAprobado = await GenerarYAprobarAsync(responsable, plantilla, "Con borrador abierto");
        Assert.Equal(HttpStatusCode.Created, (await responsable.PostAsync($"/api/documentos/{idAprobado}/versiones", null)).StatusCode);

        var r2 = await responsable.PostAsync($"/api/documentos/{idAprobado}/versiones", null);
        Assert.Equal(HttpStatusCode.Conflict, r2.StatusCode);
        Assert.Equal("documentos.ya_tiene_borrador", (await Escenario.Json(r2)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Permisos_y_aislamiento_protegen_creacion_y_consulta_de_versiones()
    {
        var (empresa, responsable, plantilla) = await EmpresaConPlantillaAsync(TipoDocumento.Politica, ContenidoInicial);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var encuestado = await _escenario.ClienteConRolAsync(empresa, "encuestado");
        var (_, otraEmpresa, _) = await EmpresaConPlantillaAsync(TipoDocumento.Politica, ContenidoInicial);

        var id1 = await GenerarYAprobarAsync(responsable, plantilla, "Doc 1");
        var id2 = await GenerarYAprobarAsync(responsable, plantilla, "Doc 2");
        var v1DeDoc1 = (await Escenario.Json(await responsable.GetAsync($"/api/documentos/{id1}")))
            .GetProperty("version").GetProperty("id").GetGuid();

        // Admin de empresa consulta historial y versión, pero NO puede crear versión nueva.
        Assert.Equal(HttpStatusCode.Forbidden, (await empresa.Cliente.PostAsync($"/api/documentos/{id1}/versiones", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.GetAsync($"/api/documentos/{id1}/versiones")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.GetAsync($"/api/documentos/{id1}/versiones/{v1DeDoc1}")).StatusCode);

        // Colaborador y encuestado reciben 403 en los tres endpoints.
        foreach (var cliente in new[] { colaborador, encuestado })
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PostAsync($"/api/documentos/{id1}/versiones", null)).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync($"/api/documentos/{id1}/versiones")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync($"/api/documentos/{id1}/versiones/{v1DeDoc1}")).StatusCode);
        }

        // Otra empresa recibe 404 en los tres endpoints (aislamiento multiempresa).
        Assert.Equal(HttpStatusCode.NotFound, (await otraEmpresa.PostAsync($"/api/documentos/{id1}/versiones", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await otraEmpresa.GetAsync($"/api/documentos/{id1}/versiones")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await otraEmpresa.GetAsync($"/api/documentos/{id1}/versiones/{v1DeDoc1}")).StatusCode);

        // Un versionId de otro documento de la misma empresa responde 404 documentos.version_no_encontrada.
        var cruceDoc = await responsable.GetAsync($"/api/documentos/{id2}/versiones/{v1DeDoc1}");
        Assert.Equal(HttpStatusCode.NotFound, cruceDoc.StatusCode);
        Assert.Equal("documentos.version_no_encontrada", (await Escenario.Json(cruceDoc)).GetProperty("codigo").GetString());
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable, Guid Plantilla)> EmpresaConPlantillaAsync(
        TipoDocumento tipo, string contenido)
    {
        var empresa = await _escenario.CrearEmpresaAsync($"Ver{Guid.NewGuid():N}"[..12], rubroId: 1);
        var responsable = await _escenario.ClienteResponsableSstAsync(empresa, $"sst.{Guid.NewGuid():N}@pruebas.hn");
        var plantilla = new PlantillaDocumento
        {
            Codigo = $"v_{Guid.NewGuid():N}"[..20],
            Titulo = "Plantilla para versiones",
            TipoDocumento = tipo,
            Contenido = contenido
        };
        await using (var db = api.NuevoContexto(empresaId: null))
        {
            db.PlantillaDocumento.Add(plantilla);
            await db.SaveChangesAsync();
        }
        return (empresa, responsable, plantilla.Id);
    }

    private static async Task<Guid> GenerarAsync(HttpClient responsable, Guid plantillaId, string titulo)
    {
        var r = await responsable.PostAsJsonAsync("/api/documentos/generacion",
            new { plantillaDocumentoId = plantillaId, titulo });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> GenerarYAprobarAsync(HttpClient responsable, Guid plantillaId, string titulo)
    {
        var id = await GenerarAsync(responsable, plantillaId, titulo);
        var r = await responsable.PostAsync($"/api/documentos/{id}/aprobacion", null);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        return id;
    }
}
