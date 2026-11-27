using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Documentos;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Documentos;

/// <summary>HU-070: generar una política o un procedimiento desde una plantilla del rubro (D13, RN-49).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class GenerarDocumentoTests(NorviaApiFactory api)
{
    private const short Oficina = 1;
    private const short Construccion = 4;
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Lista_solo_las_plantillas_activas_del_rubro_o_de_todos()
    {
        var (_, cliente) = await EmpresaConResponsableAsync(Oficina);
        var todos = await PlantillaAsync(null);
        var delRubro = await PlantillaAsync(Oficina);
        var deOtro = await PlantillaAsync(Construccion);
        var inactiva = await PlantillaAsync(Oficina, activa: false);

        var r = await cliente.GetAsync("/api/documentos/plantillas");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var ids = (await Escenario.Json(r)).EnumerateArray().Select(p => p.GetProperty("id").GetGuid()).ToList();
        Assert.Contains(todos, ids);
        Assert.Contains(delRubro, ids);
        Assert.DoesNotContain(deOtro, ids);
        Assert.DoesNotContain(inactiva, ids);
    }

    [Fact]
    public async Task Genera_un_borrador_con_los_datos_de_la_empresa_y_marca_lo_que_falta()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync(Oficina);
        await using (var db = api.NuevoContexto(empresaId: null))
        {
            var e = await db.Empresa.SingleAsync(x => x.Id == empresa.EmpresaId);
            e.ActividadEconomica = "Servicios contables"; // la dirección queda vacía
            await db.SaveChangesAsync();
        }
        const string contenido = "# Política de {{RazonSocial}}\n\nRTN {{rtn}}, en {{Direccion}}. Se dedica a {{ActividadEconomica}} ({{Rubro}}). "
            + "Emitida el {{Fecha}}. {{Desconocido}} queda igual.\n\n**Aprueba:** [Completar: nombre de quien aprueba]";
        var plantilla = await PlantillaAsync(null, contenido: contenido);

        var r = await cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var json = await Escenario.Json(r);
        var version = json.GetProperty("version");
        var texto = version.GetProperty("contenido").GetString()!;
        Assert.Equal("borrador", version.GetProperty("estado").GetString());
        Assert.Equal(1, version.GetProperty("numeroVersion").GetInt32());
        Assert.Equal("politica", json.GetProperty("tipoDocumento").GetString());
        Assert.StartsWith("# Política de Docs", texto);
        Assert.Contains("S. de R.L.\n\nRTN ", texto);
        Assert.Contains("en [Completar: dirección].", texto);
        Assert.Contains("Se dedica a Servicios contables (Oficina administrativa).", texto);
        Assert.Contains("{{Desconocido}} queda igual.", texto);
        Assert.DoesNotContain("{{Fecha}}", texto);
        Assert.Matches(@"Emitida el \d{1,2} de [a-z]+ de \d{4}\.", texto);
        Assert.Equal(new[] { "dirección", "nombre de quien aprueba" },
            json.GetProperty("porCompletar").EnumerateArray().Select(x => x.GetString()).ToArray());

        // La plantilla no cambia: el documento guarda su propia copia.
        await using var verificacion = api.NuevoContexto(empresaId: null);
        Assert.Equal(contenido, (await verificacion.PlantillaDocumento.SingleAsync(p => p.Id == plantilla)).Contenido);
    }

    [Fact]
    public async Task Toma_el_titulo_de_la_plantilla_o_el_que_se_indica_y_aparece_en_la_lista_y_en_el_detalle()
    {
        var (_, cliente) = await EmpresaConResponsableAsync(Oficina);
        var plantilla = await PlantillaAsync(null, titulo: "Política de SST");

        var sinTitulo = await Escenario.Json(await cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla }));
        var conTitulo = await Escenario.Json(await cliente.PostAsJsonAsync("/api/documentos/generacion",
            new { plantillaDocumentoId = plantilla, titulo = "  Política SST 2026 " }));

        Assert.Equal("Política de SST", sinTitulo.GetProperty("titulo").GetString());
        Assert.Equal("Política SST 2026", conTitulo.GetProperty("titulo").GetString());
        Assert.Equal("Política de SST", conTitulo.GetProperty("plantillaTitulo").GetString());

        var lista = (await Escenario.Json(await cliente.GetAsync("/api/documentos"))).EnumerateArray().ToList();
        Assert.Equal(2, lista.Count);
        Assert.All(lista, d => Assert.Equal("borrador", d.GetProperty("estado").GetString()));

        var id = conTitulo.GetProperty("id").GetGuid();
        var detalle = await cliente.GetAsync($"/api/documentos/{id}");
        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
        Assert.Equal("Política SST 2026", (await Escenario.Json(detalle)).GetProperty("titulo").GetString());
    }

    [Fact]
    public async Task Sin_rubro_en_el_perfil_no_hay_plantillas_ni_se_genera()
    {
        var (_, cliente) = await EmpresaConResponsableAsync(rubro: null);
        var plantilla = await PlantillaAsync(null);

        var plantillas = await cliente.GetAsync("/api/documentos/plantillas");
        var generar = await cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla });

        Assert.Equal(HttpStatusCode.Conflict, plantillas.StatusCode);
        Assert.Equal("documentos.sin_rubro", (await Escenario.Json(plantillas)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.Conflict, generar.StatusCode);
    }

    [Fact]
    public async Task No_genera_con_una_plantilla_de_otro_rubro_desactivada_o_inexistente()
    {
        var (_, cliente) = await EmpresaConResponsableAsync(Oficina);
        foreach (var plantilla in new[] { await PlantillaAsync(Construccion), await PlantillaAsync(Oficina, activa: false), Guid.NewGuid() })
        {
            var r = await cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla });
            Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
            Assert.Equal("documentos.plantilla_no_disponible", (await Escenario.Json(r)).GetProperty("codigo").GetString());
        }
    }

    [Fact]
    public async Task Valida_la_plantilla_y_el_largo_del_titulo()
    {
        var (_, cliente) = await EmpresaConResponsableAsync(Oficina);
        var plantilla = await PlantillaAsync(null);

        var sinPlantilla = await cliente.PostAsJsonAsync("/api/documentos/generacion", new { titulo = "Algo" });
        var largo = await cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla, titulo = new string('a', 201) });

        Assert.Equal(HttpStatusCode.BadRequest, sinPlantilla.StatusCode);
        Assert.True((await Escenario.Json(sinPlantilla)).GetProperty("errors").TryGetProperty("PlantillaDocumentoId", out _));
        Assert.Equal(HttpStatusCode.BadRequest, largo.StatusCode);
    }

    [Fact]
    public async Task El_admin_consulta_pero_no_genera_y_el_colaborador_no_ve_documentos()
    {
        var (empresa, responsable) = await EmpresaConResponsableAsync(Oficina);
        var plantilla = await PlantillaAsync(null);
        var id = (await Escenario.Json(await responsable.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla })))
            .GetProperty("id").GetGuid();
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.GetAsync("/api/documentos")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.GetAsync($"/api/documentos/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await empresa.Cliente.GetAsync("/api/documentos/plantillas")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await empresa.Cliente.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync("/api/documentos")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync($"/api/documentos/{id}")).StatusCode);
    }

    [Fact]
    public async Task Una_empresa_no_ve_los_documentos_de_otra()
    {
        var (_, clienteA) = await EmpresaConResponsableAsync(Oficina);
        var (_, clienteB) = await EmpresaConResponsableAsync(Oficina);
        var plantilla = await PlantillaAsync(null);
        var idA = (await Escenario.Json(await clienteA.PostAsJsonAsync("/api/documentos/generacion", new { plantillaDocumentoId = plantilla })))
            .GetProperty("id").GetGuid();

        var detalle = await clienteB.GetAsync($"/api/documentos/{idA}");
        var lista = await Escenario.Json(await clienteB.GetAsync("/api/documentos"));

        Assert.Equal(HttpStatusCode.NotFound, detalle.StatusCode);
        Assert.Equal("documentos.no_encontrado", (await Escenario.Json(detalle)).GetProperty("codigo").GetString());
        Assert.Equal(0, lista.GetArrayLength());
    }

    [Fact]
    public void Los_marcadores_no_distinguen_mayusculas_y_lo_por_completar_no_se_repite()
    {
        var datos = new DatosEmpresaDocumento("Acme S. A.", "08011990123456", " ", null, "Comercio o tienda");

        var texto = Marcadores.Reemplazar("{{ razonsocial }} {{RTN}} {{Direccion}} {{Direccion}} {{Rubro}} {{Fecha}}", datos, new DateOnly(2026, 11, 25));

        Assert.Equal("Acme S. A. 08011990123456 [Completar: dirección] [Completar: dirección] Comercio o tienda 25 de noviembre de 2026", texto);
        Assert.Equal(new[] { "dirección" }, Marcadores.PorCompletar(texto));
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync(short? rubro)
    {
        var empresa = await _escenario.CrearEmpresaAsync($"Docs{Guid.NewGuid():N}"[..12], rubro);
        var responsable = await _escenario.ClienteResponsableSstAsync(empresa, $"sst.{Guid.NewGuid():N}@pruebas.hn");
        return (empresa, responsable);
    }

    /// <summary>Plantilla global de documento con código único (el catálogo es compartido entre pruebas).</summary>
    private async Task<Guid> PlantillaAsync(short? rubro, bool activa = true, string titulo = "Plantilla de prueba",
        string contenido = "# {{RazonSocial}}\n\nTexto de prueba.")
    {
        var plantilla = new PlantillaDocumento
        {
            Codigo = $"p_{Guid.NewGuid():N}"[..20], Titulo = titulo, TipoDocumento = TipoDocumento.Politica, RubroId = rubro,
            Contenido = contenido, Activa = activa
        };
        await using var db = api.NuevoContexto(empresaId: null);
        db.PlantillaDocumento.Add(plantilla);
        await db.SaveChangesAsync();
        return plantilla.Id;
    }
}
