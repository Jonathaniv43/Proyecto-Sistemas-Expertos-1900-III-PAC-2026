using System.Net;
using System.Net.Http.Json;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Riesgos;

/// <summary>HU-041: historial de evaluaciones de un riesgo.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class HistorialRiesgoTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Muestra_todas_las_evaluaciones_iniciales_y_residuales_con_fecha_y_autor_la_mas_reciente_primero()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarAsync(cliente);
        await Evaluar(cliente, riesgo, "evaluaciones", 4, 4);
        (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/controles", new { tipo = "ingenieria", descripcion = "Guarda" })).EnsureSuccessStatusCode();
        await Evaluar(cliente, riesgo, "evaluaciones-residual", 2, 3);
        await Evaluar(cliente, riesgo, "evaluaciones", 5, 4); // reevaluación inicial: no borra la anterior (RN-20)

        var historial = (await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}/evaluaciones"))).EnumerateArray().ToList();

        Assert.Equal(new[] { 20, 6, 16 }, historial.Select(e => e.GetProperty("nivel").GetInt32()));
        Assert.Equal(new[] { "inicial", "residual", "inicial" }, historial.Select(e => e.GetProperty("tipo").GetString()));
        Assert.All(historial, e =>
        {
            Assert.Equal("Persona Prueba", e.GetProperty("evaluadoPorNombre").GetString());
            Assert.True(e.GetProperty("evaluadoEn").GetDateTime() > DateTime.UtcNow.AddMinutes(-5));
        });
        Assert.Equal("critico", historial[0].GetProperty("clasificacion").GetString());
    }

    [Fact]
    public async Task Un_riesgo_sin_evaluar_tiene_el_historial_vacio()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarAsync(cliente);

        var r = await cliente.GetAsync($"/api/riesgos/{riesgo}/evaluaciones");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Empty((await Escenario.Json(r)).EnumerateArray());
    }

    [Fact]
    public async Task Otra_empresa_no_ve_el_historial_y_un_colaborador_no_tiene_permiso()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarAsync(cliente);
        await Evaluar(cliente, riesgo, "evaluaciones", 3, 3);
        var (_, ajeno) = await EmpresaConResponsableAsync();
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        Assert.Equal(HttpStatusCode.NotFound, (await ajeno.GetAsync($"/api/riesgos/{riesgo}/evaluaciones")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync($"/api/riesgos/{riesgo}/evaluaciones")).StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Historial", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    private static async Task<Guid> AgregarAsync(HttpClient cliente)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = $"Peligro {Guid.NewGuid():N}"[..20], actividad = "Planta" });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task Evaluar(HttpClient cliente, Guid riesgo, string ruta, int probabilidad, int severidad) =>
        (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/{ruta}", new { probabilidad, severidad })).EnsureSuccessStatusCode();
}
