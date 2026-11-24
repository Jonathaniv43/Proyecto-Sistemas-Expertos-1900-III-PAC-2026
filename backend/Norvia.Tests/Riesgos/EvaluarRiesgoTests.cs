using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Riesgos;

/// <summary>HU-037: evaluar un riesgo (probabilidad × severidad) y conservar quién y cuándo.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class EvaluarRiesgoTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Theory]
    [InlineData(1, 4, 4, "bajo")]
    [InlineData(1, 5, 5, "medio")]
    [InlineData(3, 3, 9, "medio")]
    [InlineData(2, 5, 10, "alto")]
    [InlineData(3, 4, 12, "alto")]
    [InlineData(3, 5, 15, "critico")]
    [InlineData(5, 5, 25, "critico")]
    public async Task Calcula_el_nivel_y_lo_clasifica_segun_RN_16(int probabilidad, int severidad, int nivel, string clasificacion)
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgoId = await AgregarRiesgoAsync(cliente);

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { probabilidad, severidad });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal(nivel, json.GetProperty("nivel").GetInt32());
        Assert.Equal(clasificacion, json.GetProperty("clasificacion").GetString());
        Assert.Equal("inicial", json.GetProperty("tipo").GetString());
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(6, 3)]
    [InlineData(3, 0)]
    [InlineData(3, 6)]
    public async Task Rechaza_valores_fuera_de_la_escala(int probabilidad, int severidad)
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgoId = await AgregarRiesgoAsync(cliente);

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { probabilidad, severidad });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Rechaza_una_evaluacion_sin_probabilidad_o_sin_severidad()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgoId = await AgregarRiesgoAsync(cliente);

        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { probabilidad = 3 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { severidad = 3 })).StatusCode);
    }

    [Fact]
    public async Task Guarda_quien_evaluo_y_cuando_y_una_nueva_no_reemplaza_la_anterior()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var riesgoId = await AgregarRiesgoAsync(cliente);

        await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { probabilidad = 2, severidad = 2 });
        var segunda = await Escenario.Json(await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { probabilidad = 4, severidad = 3 }));

        Assert.Equal("Persona Prueba", segunda.GetProperty("evaluadoPorNombre").GetString());
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            var evaluaciones = await db.EvaluacionRiesgo.AsNoTracking().Where(e => e.RiesgoEmpresaId == riesgoId).ToListAsync();
            Assert.Equal(2, evaluaciones.Count); // RN-20: se conserva el historial
            Assert.All(evaluaciones, e => Assert.NotEqual(default, e.EvaluadoEn));
        }

        // La matriz y el detalle muestran la más reciente, con su evaluador.
        var detalle = await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgoId}"));
        var evaluacion = detalle.GetProperty("evaluacion");
        Assert.Equal(12, evaluacion.GetProperty("nivel").GetInt32());
        Assert.Equal("alto", evaluacion.GetProperty("clasificacion").GetString());
        Assert.Equal("Persona Prueba", evaluacion.GetProperty("evaluadoPorNombre").GetString());
        var enLista = (await Escenario.Json(await cliente.GetAsync("/api/riesgos"))).EnumerateArray()
            .Single(x => x.GetProperty("id").GetGuid() == riesgoId);
        Assert.Equal(12, enLista.GetProperty("evaluacion").GetProperty("nivel").GetInt32());
    }

    [Fact]
    public async Task Un_riesgo_sin_evaluar_no_trae_evaluacion()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgoId = await AgregarRiesgoAsync(cliente);

        var detalle = await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgoId}"));

        Assert.Equal(JsonValueKind.Null, detalle.GetProperty("evaluacion").ValueKind);
    }

    [Fact]
    public async Task Un_riesgo_que_no_esta_en_la_matriz_no_se_evalua()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        Guid sugerido;
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            var peligro = new Peligro { Codigo = $"p{Guid.NewGuid():N}"[..20], Nombre = "Ruido", Clasificacion = ClasificacionPeligro.Fisico };
            var encuesta = new Encuesta { Titulo = "Diagnóstico", Estado = EstadoEncuesta.Publicada, CreadaPorId = empresa.AdminVinculoId };
            var riesgo = new RiesgoEmpresa { PeligroId = peligro.Id, EncuestaId = encuesta.Id, Estado = EstadoRiesgo.Sugerido };
            db.AddRange(peligro, encuesta, riesgo);
            await db.SaveChangesAsync();
            sugerido = riesgo.Id;
        }

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{sugerido}/evaluaciones", new { probabilidad = 3, severidad = 3 });

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("riesgos.no_en_matriz", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Otra_empresa_no_puede_evaluar_un_riesgo_ajeno()
    {
        var (empresaA, clienteA) = await EmpresaConResponsableAsync();
        var (_, clienteB) = await EmpresaConResponsableAsync();
        var riesgoA = await AgregarRiesgoAsync(clienteA);

        var r = await clienteB.PostAsJsonAsync($"/api/riesgos/{riesgoA}/evaluaciones", new { probabilidad = 3, severidad = 3 });

        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        await using var db = api.NuevoContexto(empresaA.EmpresaId);
        Assert.False(await db.EvaluacionRiesgo.AnyAsync(e => e.RiesgoEmpresaId == riesgoA));
    }

    [Fact]
    public async Task Un_colaborador_no_puede_evaluar()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var riesgoId = await AgregarRiesgoAsync(cliente);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        var r = await colaborador.PostAsJsonAsync($"/api/riesgos/{riesgoId}/evaluaciones", new { probabilidad = 3, severidad = 3 });

        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Evaluar", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    private static async Task<Guid> AgregarRiesgoAsync(HttpClient cliente)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = $"Peligro {Guid.NewGuid():N}"[..20], actividad = "Bodega" });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }
}
