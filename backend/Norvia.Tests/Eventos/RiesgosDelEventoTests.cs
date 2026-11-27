using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Eventos;

/// <summary>HU-051: enlazar un evento con la matriz: riesgos que revela y riesgos que se reevalúan a raíz de él.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class RiesgosDelEventoTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));

    [Fact]
    public async Task Un_riesgo_nuevo_queda_enlazado_al_evento_que_lo_revelo()
    {
        var (_, sst) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);

        var r = await CrearRiesgo(sst, "Piso resbaloso", evento);
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var riesgo = (await Escenario.Json(r)).GetProperty("id").GetGuid();
        Assert.Equal(evento, (await Escenario.Json(await sst.GetAsync($"/api/riesgos/{riesgo}"))).GetProperty("accidenteId").GetGuid());

        var sinEvaluar = Assert.Single((await Riesgos(sst, evento)).EnumerateArray());
        Assert.Equal("revelado", sinEvaluar.GetProperty("relacion").GetString());
        Assert.Equal("Piso resbaloso", sinEvaluar.GetProperty("nombre").GetString());
        Assert.Equal(JsonValueKind.Null, sinEvaluar.GetProperty("nivel").ValueKind);

        (await Evaluar(sst, riesgo, 3, 4, evento)).EnsureSuccessStatusCode();
        var evaluado = Assert.Single((await Riesgos(sst, evento)).EnumerateArray());
        Assert.Equal("revelado", evaluado.GetProperty("relacion").GetString());
        Assert.Equal(12, evaluado.GetProperty("nivel").GetInt32());
        Assert.Equal("alto", evaluado.GetProperty("clasificacion").GetString());
        Assert.Equal(JsonValueKind.Null, evaluado.GetProperty("nivelAnterior").ValueKind);
    }

    [Fact]
    public async Task Reevaluar_un_riesgo_existente_guarda_el_evento_y_el_nivel_de_antes_y_despues()
    {
        var (_, sst) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        var otroEvento = await ReportarAsync(sst);
        var riesgo = await IdAsync(CrearRiesgo(sst, "Cajas apiladas sin sujetar"));
        var sinEvento = await IdAsync(CrearRiesgo(sst, "Cables sueltos"));
        (await Evaluar(sst, riesgo, 2, 2)).EnsureSuccessStatusCode();
        (await Evaluar(sst, sinEvento, 2, 2)).EnsureSuccessStatusCode();

        var r = await Evaluar(sst, riesgo, 3, 4, evento);
        (await Evaluar(sst, riesgo, 1, 2)).EnsureSuccessStatusCode(); // otra después, sin evento
        (await Evaluar(sst, riesgo, 5, 5, otroEvento)).EnsureSuccessStatusCode();

        Assert.Equal(evento, (await Escenario.Json(r)).GetProperty("accidenteId").GetGuid());
        var enlazado = Assert.Single((await Riesgos(sst, evento)).EnumerateArray());
        Assert.Equal(riesgo, enlazado.GetProperty("riesgoId").GetGuid());
        Assert.Equal("reevaluado", enlazado.GetProperty("relacion").GetString());
        Assert.Equal(4, enlazado.GetProperty("nivelAnterior").GetInt32());
        Assert.Equal("bajo", enlazado.GetProperty("clasificacionAnterior").GetString());
        Assert.Equal(12, enlazado.GetProperty("nivel").GetInt32());

        // El historial del riesgo dice qué evaluación se hizo a raíz de qué evento.
        var historial = (await Escenario.Json(await sst.GetAsync($"/api/riesgos/{riesgo}/evaluaciones"))).EnumerateArray().ToList();
        Assert.Equal([otroEvento, (Guid?)null, evento, null],
            historial.Select(e => e.GetProperty("accidenteId").ValueKind == JsonValueKind.Null ? (Guid?)null : e.GetProperty("accidenteId").GetGuid()));
        Assert.Equal(1, (await Riesgos(sst, otroEvento)).GetArrayLength());
    }

    [Fact]
    public async Task Primero_los_revelados_y_luego_los_reevaluados()
    {
        var (_, sst) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        var existente = await IdAsync(CrearRiesgo(sst, "Escalera floja"));
        (await Evaluar(sst, existente, 2, 3, evento)).EnsureSuccessStatusCode();
        await IdAsync(CrearRiesgo(sst, "Derrame de aceite", evento));

        var lista = (await Riesgos(sst, evento)).EnumerateArray().ToList();
        Assert.Equal(["revelado", "reevaluado"], lista.Select(x => x.GetProperty("relacion").GetString()));
        // Sin evaluación inicial previa, el nivel de antes queda vacío.
        Assert.Equal(JsonValueKind.Null, lista[1].GetProperty("nivelAnterior").ValueKind);
        Assert.Equal(6, lista[1].GetProperty("nivel").GetInt32());
    }

    [Fact]
    public async Task No_se_enlaza_con_un_evento_descartado_ni_con_uno_de_otra_empresa()
    {
        var (empresa, sst) = await EscenarioAsync();
        var descartado = await ReportarAsync(sst);
        (await sst.PatchAsJsonAsync($"/api/eventos/{descartado}/descarte", new { })).EnsureSuccessStatusCode();
        var (_, sstAjeno) = await EscenarioAsync();
        var ajeno = await ReportarAsync(sstAjeno);
        var riesgo = await IdAsync(CrearRiesgo(sst, "Ruido de la compresora"));

        foreach (var evento in new[] { descartado, ajeno })
        {
            Assert.Equal("riesgos.evento_invalido", (await Escenario.Json(await CrearRiesgo(sst, $"Riesgo {evento}", evento))).GetProperty("codigo").GetString());
            Assert.Equal("riesgos.evento_invalido", (await Escenario.Json(await Evaluar(sst, riesgo, 2, 2, evento))).GetProperty("codigo").GetString());
        }
        Assert.Equal(HttpStatusCode.BadRequest, (await CrearRiesgo(sst, "Vacío", Guid.Empty)).StatusCode);
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(1, db.RiesgoEmpresa.Count());
        Assert.Empty(db.EvaluacionRiesgo);
    }

    [Fact]
    public async Task Ver_los_riesgos_del_evento_es_de_la_gestion_y_de_la_misma_empresa()
    {
        var (empresa, sst) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        await IdAsync(CrearRiesgo(sst, "Piso resbaloso", evento));
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var (_, sstAjeno) = await EscenarioAsync();

        Assert.Equal(1, (await Riesgos(empresa.Cliente, evento)).GetArrayLength()); // admin: consulta
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync($"/api/eventos/{evento}/riesgos")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await sstAjeno.GetAsync($"/api/eventos/{evento}/riesgos")).StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Sst)> EscenarioAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("RiesgosEvento", 1);
        var email = $"sst.{Guid.NewGuid():N}@pruebas.hn";
        (await empresa.Cliente.PostAsJsonAsync("/api/vinculos", new
        {
            email, primerNombre = "Persona", primerApellido = "Prueba", rolCodigo = "responsable_sst"
        })).EnsureSuccessStatusCode();
        await Escenario.ActivarCuentaAsync(empresa, email, "Temporal#2026");
        return (empresa, await _escenario.ClienteEmpresaAsync(email, "Temporal#2026", empresa.IdEmpresaCifrado));
    }

    private static async Task<Guid> ReportarAsync(HttpClient cliente)
    {
        var r = await cliente.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "accidente", esGrave = false, descripcion = "Resbalón en el pasillo",
            fechaHoraEvento = $"{Hoy.AddDays(-3):yyyy-MM-dd}T10:00:00-06:00"
        });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static Task<HttpResponseMessage> CrearRiesgo(HttpClient cliente, string peligro, Guid? evento = null) =>
        cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = peligro, actividad = "Bodega", accidenteId = evento });

    private static Task<HttpResponseMessage> Evaluar(HttpClient cliente, Guid riesgo, short p, short s, Guid? evento = null) =>
        cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/evaluaciones", new { probabilidad = p, severidad = s, accidenteId = evento });

    private static async Task<JsonElement> Riesgos(HttpClient cliente, Guid evento)
    {
        var r = await cliente.GetAsync($"/api/eventos/{evento}/riesgos");
        r.EnsureSuccessStatusCode();
        return await Escenario.Json(r);
    }

    private static async Task<Guid> IdAsync(Task<HttpResponseMessage> respuesta)
    {
        var r = await respuesta;
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }
}
