using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Riesgos;

/// <summary>HU-038: mapa de la matriz y lista filtrable de riesgos.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class MatrizRiesgosTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task El_mapa_cuenta_los_riesgos_de_cada_celda_por_su_ultima_evaluacion()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        await AgregarEvaluadoAsync(cliente, "Ruido", 3, 4);
        await AgregarEvaluadoAsync(cliente, "Polvo", 3, 4);
        var reevaluado = await AgregarEvaluadoAsync(cliente, "Caída", 2, 2);
        await Evaluar(cliente, reevaluado, 5, 5); // cuenta la última
        await AgregarAsync(cliente, "Sin evaluar");

        var m = await Escenario.Json(await cliente.GetAsync("/api/riesgos/matriz"));

        Assert.Equal(4, m.GetProperty("total").GetInt32());
        Assert.Equal(1, m.GetProperty("sinEvaluar").GetInt32());
        var celdas = m.GetProperty("celdas").EnumerateArray().ToList();
        Assert.Equal(2, celdas.Count);
        var c12 = celdas.Single(c => c.GetProperty("probabilidad").GetInt32() == 3 && c.GetProperty("severidad").GetInt32() == 4);
        Assert.Equal(2, c12.GetProperty("cantidad").GetInt32());
        Assert.Equal("alto", c12.GetProperty("clasificacion").GetString());
        Assert.DoesNotContain(celdas, c => c.GetProperty("probabilidad").GetInt32() == 2);
    }

    [Fact]
    public async Task La_lista_va_del_nivel_mas_alto_al_mas_bajo_y_los_sin_evaluar_al_final()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        await AgregarAsync(cliente, "Sin evaluar");
        await AgregarEvaluadoAsync(cliente, "Medio", 2, 3);
        await AgregarEvaluadoAsync(cliente, "Crítico", 4, 5);

        var nombres = await Nombres(cliente, "");

        Assert.Equal(new[] { "Crítico", "Medio", "Sin evaluar" }, nombres);
    }

    [Fact]
    public async Task Filtra_por_texto_clasificacion_del_peligro_nivel_sin_evaluar_y_celda()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var locativo = await CrearPeligroAsync("Caída de escalera", ClasificacionPeligro.Locativo);
        var r1 = await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId = locativo, actividad = "Bodega" });
        await Evaluar(cliente, (await Escenario.Json(r1)).GetProperty("id").GetGuid(), 3, 4);
        await AgregarEvaluadoAsync(cliente, "Cables sueltos", 1, 2);
        await AgregarAsync(cliente, "Ruido de compresor");

        Assert.Equal(new[] { "Caída de escalera" }, await Nombres(cliente, "?buscar=ESCALERA"));
        Assert.Equal(new[] { "Caída de escalera" }, await Nombres(cliente, "?buscar=bodega")); // también la actividad
        Assert.Equal(new[] { "Caída de escalera" }, await Nombres(cliente, "?clasificacionPeligro=locativo"));
        Assert.Equal(new[] { "Caída de escalera" }, await Nombres(cliente, "?nivel=alto"));
        Assert.Equal(new[] { "Cables sueltos" }, await Nombres(cliente, "?nivel=bajo"));
        Assert.Equal(new[] { "Ruido de compresor" }, await Nombres(cliente, "?sinEvaluar=true"));
        Assert.Equal(new[] { "Caída de escalera" }, await Nombres(cliente, "?probabilidad=3&severidad=4"));
        Assert.Empty(await Nombres(cliente, "?probabilidad=4&severidad=3"));
    }

    [Fact]
    public async Task Por_estado_lista_los_sugeridos_o_descartados_que_no_estan_en_la_matriz()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var sugerido = await CrearPeligroAsync("Ruido sugerido", ClasificacionPeligro.Fisico);
        var descartado = await CrearPeligroAsync("Polvo descartado", ClasificacionPeligro.Quimico);
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            var encuesta = new Encuesta { Titulo = "Diagnóstico", Estado = EstadoEncuesta.Publicada, CreadaPorId = empresa.AdminVinculoId };
            db.Encuesta.Add(encuesta);
            db.RiesgoEmpresa.AddRange(
                new RiesgoEmpresa { PeligroId = sugerido, EncuestaId = encuesta.Id, Estado = EstadoRiesgo.Sugerido },
                new RiesgoEmpresa { PeligroId = descartado, EncuestaId = encuesta.Id, Estado = EstadoRiesgo.Descartado });
            await db.SaveChangesAsync();
        }
        await AgregarAsync(cliente, "En la matriz");

        Assert.Equal(new[] { "En la matriz" }, await Nombres(cliente, ""));
        Assert.Equal(new[] { "Ruido sugerido" }, await Nombres(cliente, "?estado=sugerido"));
        Assert.Equal(new[] { "Polvo descartado" }, await Nombres(cliente, "?estado=descartado"));
        Assert.Equal(1, (await Escenario.Json(await cliente.GetAsync("/api/riesgos/matriz"))).GetProperty("total").GetInt32());
    }

    [Theory]
    [InlineData("?probabilidad=3")]
    [InlineData("?probabilidad=6&severidad=1")]
    [InlineData("?sinEvaluar=true&nivel=alto")]
    public async Task Rechaza_filtros_invalidos(string consulta)
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.GetAsync($"/api/riesgos{consulta}")).StatusCode);
    }

    [Fact]
    public async Task El_mapa_y_la_lista_no_incluyen_riesgos_de_otra_empresa()
    {
        var (_, clienteA) = await EmpresaConResponsableAsync();
        var (_, clienteB) = await EmpresaConResponsableAsync();
        await AgregarEvaluadoAsync(clienteA, "De A", 3, 3);

        var matrizB = await Escenario.Json(await clienteB.GetAsync("/api/riesgos/matriz"));

        Assert.Equal(0, matrizB.GetProperty("total").GetInt32());
        Assert.Empty(matrizB.GetProperty("celdas").EnumerateArray());
        Assert.Empty(await Nombres(clienteB, ""));
    }

    [Fact]
    public async Task Un_colaborador_no_ve_el_mapa()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Matriz", 1);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync("/api/riesgos/matriz")).StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Matriz", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    private async Task<Guid> CrearPeligroAsync(string nombre, ClasificacionPeligro clasificacion)
    {
        var peligro = new Peligro { Codigo = $"p{Guid.NewGuid():N}"[..20], Nombre = nombre, Clasificacion = clasificacion };
        await using var db = api.NuevoContexto(empresaId: null);
        db.Peligro.Add(peligro);
        await db.SaveChangesAsync();
        return peligro.Id;
    }

    private static async Task<Guid> AgregarAsync(HttpClient cliente, string descripcion)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = descripcion, actividad = "Planta" });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task Evaluar(HttpClient cliente, Guid riesgoId, int probabilidad, int severidad) =>
        (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { probabilidad, severidad })).EnsureSuccessStatusCode();

    private static async Task<Guid> AgregarEvaluadoAsync(HttpClient cliente, string descripcion, int probabilidad, int severidad)
    {
        var id = await AgregarAsync(cliente, descripcion);
        await Evaluar(cliente, id, probabilidad, severidad);
        return id;
    }

    private static async Task<string[]> Nombres(HttpClient cliente, string consulta) =>
        (await Escenario.Json(await cliente.GetAsync($"/api/riesgos{consulta}"))).EnumerateArray()
            .Select(x => x.GetProperty("peligroNombre").GetString() ?? x.GetProperty("descripcionPeligro").GetString()!)
            .ToArray();
}
