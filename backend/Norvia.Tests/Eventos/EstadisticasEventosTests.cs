using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Eventos;

/// <summary>HU-052: estadísticas de eventos por tipo, gravedad y mes; sin descartados ni datos de personas.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class EstadisticasEventosTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));

    [Fact]
    public async Task Cuenta_por_tipo_gravedad_y_mes_sin_descartados()
    {
        var (_, sst) = await EscenarioAsync();
        var mesActual = new DateOnly(Hoy.Year, Hoy.Month, 1);
        var mesAnterior = mesActual.AddMonths(-1);
        await Reportar(sst, "accidente", true, mesAnterior.AddDays(2));
        await Reportar(sst, "accidente", false, mesAnterior.AddDays(5));
        await Reportar(sst, "incidente", false, mesActual);
        await Reportar(sst, "condicion_insegura", true, Hoy);
        var descartado = await Reportar(sst, "accidente", true, Hoy);
        (await sst.PatchAsJsonAsync($"/api/eventos/{descartado}/descarte", new { })).EnsureSuccessStatusCode();
        await Reportar(sst, "incidente", true, mesAnterior.AddDays(-1)); // fuera del periodo

        var json = await Estadisticas(sst, mesAnterior, Hoy);

        Assert.Equal(4, json.GetProperty("total").GetInt32());
        Assert.Equal(2, json.GetProperty("graves").GetInt32());
        var porTipo = json.GetProperty("porTipo").EnumerateArray()
            .Select(t => (t.GetProperty("tipo").GetString(), t.GetProperty("cantidad").GetInt32(), t.GetProperty("graves").GetInt32())).ToList();
        Assert.Equal([("accidente", 2, 1), ("incidente", 1, 0), ("enfermedad_profesional", 0, 0), ("condicion_insegura", 1, 1)], porTipo);
        var porMes = json.GetProperty("porMes").EnumerateArray()
            .Select(m => (m.GetProperty("anio").GetInt32(), m.GetProperty("mes").GetInt32(), m.GetProperty("cantidad").GetInt32())).ToList();
        Assert.Equal([(mesAnterior.Year, mesAnterior.Month, 2), (mesActual.Year, mesActual.Month, 2)], porMes);
    }

    [Fact]
    public async Task El_periodo_usa_la_fecha_de_Honduras_e_incluye_los_dos_extremos()
    {
        var (_, sst) = await EscenarioAsync();
        var desde = Hoy.AddDays(-10);
        var hasta = Hoy.AddDays(-5);
        await Reportar(sst, "incidente", false, desde.AddDays(-1), "23:50"); // 05:50 UTC del día "desde": fuera
        await Reportar(sst, "incidente", false, desde, "00:10");
        await Reportar(sst, "incidente", false, hasta, "23:50");            // 05:50 UTC del día siguiente: dentro
        await Reportar(sst, "incidente", false, hasta.AddDays(1), "00:10");

        Assert.Equal(2, (await Estadisticas(sst, desde, hasta)).GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task Solo_cifras_sin_datos_de_personas_y_con_todos_los_meses()
    {
        var (_, sst) = await EscenarioAsync();
        await Reportar(sst, "accidente", true, Hoy);

        var desde = Hoy.AddMonths(-24).AddDays(1);
        var json = await Estadisticas(sst, desde, Hoy);

        Assert.Equal(["desde", "hasta", "total", "graves", "porTipo", "porMes"], json.EnumerateObject().Select(p => p.Name));
        Assert.Equal(["tipo", "cantidad", "graves"], json.GetProperty("porTipo")[0].EnumerateObject().Select(p => p.Name));
        Assert.Equal(["anio", "mes", "cantidad", "graves"], json.GetProperty("porMes")[0].EnumerateObject().Select(p => p.Name));
        var meses = json.GetProperty("porMes").EnumerateArray().ToList();
        // Del mes de "desde" al de hoy, también los vacíos.
        Assert.Equal((Hoy.Year - desde.Year) * 12 + Hoy.Month - desde.Month + 1, meses.Count);
        Assert.Equal(1, meses[^1].GetProperty("cantidad").GetInt32());
        Assert.Equal(0, meses[0].GetProperty("cantidad").GetInt32());
    }

    [Fact]
    public async Task Valida_el_periodo()
    {
        var (_, sst) = await EscenarioAsync();

        var sinFechas = await sst.GetAsync("/api/eventos/estadisticas");
        var alReves = await sst.GetAsync($"/api/eventos/estadisticas?desde={Hoy:yyyy-MM-dd}&hasta={Hoy.AddDays(-1):yyyy-MM-dd}");
        var largo = await sst.GetAsync($"/api/eventos/estadisticas?desde={Hoy.AddMonths(-24):yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.BadRequest, sinFechas.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, alReves.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, largo.StatusCode);
    }

    [Fact]
    public async Task Es_de_la_gestion_y_solo_cuenta_la_empresa_de_la_sesion()
    {
        var (empresa, sst) = await EscenarioAsync();
        await Reportar(sst, "accidente", false, Hoy);
        var (_, sstAjeno) = await EscenarioAsync();
        await Reportar(sstAjeno, "accidente", false, Hoy);
        await Reportar(sstAjeno, "incidente", false, Hoy);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        Assert.Equal(1, (await Estadisticas(empresa.Cliente, Hoy, Hoy)).GetProperty("total").GetInt32()); // admin
        Assert.Equal(2, (await Estadisticas(sstAjeno, Hoy, Hoy)).GetProperty("total").GetInt32());
        Assert.Equal(HttpStatusCode.Forbidden,
            (await colaborador.GetAsync($"/api/eventos/estadisticas?desde={Hoy:yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}")).StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Sst)> EscenarioAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Estadisticas", 1);
        var email = $"sst.{Guid.NewGuid():N}@pruebas.hn";
        (await empresa.Cliente.PostAsJsonAsync("/api/vinculos", new
        {
            email, primerNombre = "Persona", primerApellido = "Prueba", rolCodigo = "responsable_sst", passwordTemporal = "Temporal#2026"
        })).EnsureSuccessStatusCode();
        return (empresa, await _escenario.ClienteEmpresaAsync(email, "Temporal#2026", empresa.IdEmpresaCifrado));
    }

    /// <summary>Reporta un evento a la hora indicada de Honduras (por defecto, temprano para que nunca sea futuro).</summary>
    private static async Task<Guid> Reportar(HttpClient cliente, string tipo, bool grave, DateOnly fecha, string hora = "00:05")
    {
        var r = await cliente.PostAsJsonAsync("/api/eventos", new
        {
            tipo, esGrave = grave, descripcion = "Evento de prueba",
            estadoEnfermedad = tipo == "enfermedad_profesional" ? "sospechosa" : null,
            fechaHoraEvento = $"{fecha:yyyy-MM-dd}T{hora}:00-06:00"
        });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement> Estadisticas(HttpClient cliente, DateOnly desde, DateOnly hasta)
    {
        var r = await cliente.GetAsync($"/api/eventos/estadisticas?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}");
        r.EnsureSuccessStatusCode();
        return await Escenario.Json(r);
    }
}
