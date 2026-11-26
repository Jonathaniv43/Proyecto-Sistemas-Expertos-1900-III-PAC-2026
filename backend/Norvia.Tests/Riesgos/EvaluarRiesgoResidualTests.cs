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

/// <summary>HU-040: evaluación residual, separada de la inicial y visible junto a ella.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class EvaluarRiesgoResidualTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Se_guarda_aparte_de_la_inicial_y_el_riesgo_trae_las_dos()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await RiesgoListoAsync(cliente, 4, 4);

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/evaluaciones-residual", new { probabilidad = 2, severidad = 3 });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var residual = await Escenario.Json(r);
        Assert.Equal("residual", residual.GetProperty("tipo").GetString());
        Assert.Equal(6, residual.GetProperty("nivel").GetInt32());
        Assert.Equal("medio", residual.GetProperty("clasificacion").GetString());
        Assert.Equal("Persona Prueba", residual.GetProperty("evaluadoPorNombre").GetString());

        var detalle = await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}"));
        Assert.Equal(16, detalle.GetProperty("evaluacion").GetProperty("nivel").GetInt32()); // la inicial no cambia
        Assert.Equal("inicial", detalle.GetProperty("evaluacion").GetProperty("tipo").GetString());
        Assert.Equal(6, detalle.GetProperty("evaluacionResidual").GetProperty("nivel").GetInt32());
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(2, await db.EvaluacionRiesgo.CountAsync(e => e.RiesgoEmpresaId == riesgo));
    }

    [Fact]
    public async Task Una_residual_nueva_no_borra_la_anterior_y_el_riesgo_muestra_la_ultima()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await RiesgoListoAsync(cliente, 5, 5);

        await Residual(cliente, riesgo, 3, 3);
        await Residual(cliente, riesgo, 1, 4);
        var detalle = await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}"));
        var enLista = (await Escenario.Json(await cliente.GetAsync("/api/riesgos"))).EnumerateArray()
            .Single(x => x.GetProperty("id").GetGuid() == riesgo);

        Assert.Equal(4, detalle.GetProperty("evaluacionResidual").GetProperty("nivel").GetInt32());
        Assert.Equal("bajo", enLista.GetProperty("evaluacionResidual").GetProperty("clasificacion").GetString());
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(2, await db.EvaluacionRiesgo.CountAsync(e => e.RiesgoEmpresaId == riesgo && e.Tipo == TipoEvaluacion.Residual)); // RN-20
    }

    [Fact]
    public async Task La_matriz_sigue_ubicando_por_la_evaluacion_inicial()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await RiesgoListoAsync(cliente, 4, 5);
        await Residual(cliente, riesgo, 1, 1);

        var celdas = (await Escenario.Json(await cliente.GetAsync("/api/riesgos/matriz"))).GetProperty("celdas").EnumerateArray().ToList();

        var celda = Assert.Single(celdas);
        Assert.Equal(4, celda.GetProperty("probabilidad").GetInt32());
        Assert.Equal(5, celda.GetProperty("severidad").GetInt32());
    }

    [Fact]
    public async Task Un_riesgo_sin_residual_la_trae_en_null()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await RiesgoListoAsync(cliente, 3, 3);

        var detalle = await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}"));

        Assert.Equal(JsonValueKind.Null, detalle.GetProperty("evaluacionResidual").ValueKind);
    }

    [Fact]
    public async Task Exige_la_evaluacion_inicial_y_al_menos_un_control()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var sinInicial = await AgregarAsync(cliente);
        await Control(cliente, sinInicial);
        var sinControles = await AgregarAsync(cliente);
        await Evaluar(cliente, sinControles, 3, 4);

        var r1 = await cliente.PostAsJsonAsync($"/api/riesgos/{sinInicial}/evaluaciones-residual", new { probabilidad = 2, severidad = 2 });
        var r2 = await cliente.PostAsJsonAsync($"/api/riesgos/{sinControles}/evaluaciones-residual", new { probabilidad = 2, severidad = 2 });

        Assert.Equal(HttpStatusCode.Conflict, r1.StatusCode);
        Assert.Equal("riesgos.sin_evaluacion_inicial", (await Escenario.Json(r1)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.Conflict, r2.StatusCode);
        Assert.Equal("riesgos.sin_controles", (await Escenario.Json(r2)).GetProperty("codigo").GetString());
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(3, 6)]
    [InlineData(null, 3)]
    public async Task Rechaza_valores_fuera_de_la_escala_o_faltantes(int? probabilidad, int severidad)
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await RiesgoListoAsync(cliente, 3, 3);

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/evaluaciones-residual", new { probabilidad, severidad });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
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

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{sugerido}/evaluaciones-residual", new { probabilidad = 2, severidad = 2 });

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("riesgos.no_en_matriz", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Otra_empresa_no_puede_evaluar_y_un_colaborador_no_tiene_permiso()
    {
        var (empresa, clienteA) = await EmpresaConResponsableAsync();
        var (_, clienteB) = await EmpresaConResponsableAsync();
        var riesgo = await RiesgoListoAsync(clienteA, 3, 4);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        var ajena = await clienteB.PostAsJsonAsync($"/api/riesgos/{riesgo}/evaluaciones-residual", new { probabilidad = 1, severidad = 1 });
        var sinPermiso = await colaborador.PostAsJsonAsync($"/api/riesgos/{riesgo}/evaluaciones-residual", new { probabilidad = 1, severidad = 1 });

        Assert.Equal(HttpStatusCode.NotFound, ajena.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, sinPermiso.StatusCode);
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.False(await db.EvaluacionRiesgo.AnyAsync(e => e.RiesgoEmpresaId == riesgo && e.Tipo == TipoEvaluacion.Residual));
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Residual", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    private static async Task<Guid> AgregarAsync(HttpClient cliente)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = $"Peligro {Guid.NewGuid():N}"[..20], actividad = "Planta" });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task Evaluar(HttpClient cliente, Guid id, int probabilidad, int severidad) =>
        (await cliente.PostAsJsonAsync($"/api/riesgos/{id}/evaluaciones", new { probabilidad, severidad })).EnsureSuccessStatusCode();

    private static async Task Control(HttpClient cliente, Guid id) =>
        (await cliente.PostAsJsonAsync($"/api/riesgos/{id}/controles", new { tipo = "ingenieria", descripcion = "Guarda" })).EnsureSuccessStatusCode();

    private static async Task Residual(HttpClient cliente, Guid id, int probabilidad, int severidad) =>
        (await cliente.PostAsJsonAsync($"/api/riesgos/{id}/evaluaciones-residual", new { probabilidad, severidad })).EnsureSuccessStatusCode();

    /// <summary>Riesgo confirmado, con evaluación inicial y un control: listo para la residual.</summary>
    private static async Task<Guid> RiesgoListoAsync(HttpClient cliente, int probabilidad, int severidad)
    {
        var id = await AgregarAsync(cliente);
        await Evaluar(cliente, id, probabilidad, severidad);
        await Control(cliente, id);
        return id;
    }
}
