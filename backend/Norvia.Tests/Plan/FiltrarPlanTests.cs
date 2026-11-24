using System.Net;
using System.Net.Http.Json;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Plan;

/// <summary>HU-046: filtrar el plan por estado, vencidas, responsable, origen, prioridad y fecha límite.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class FiltrarPlanTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));

    [Fact]
    public async Task Filtra_por_estado_responsable_origen_prioridad_y_fecha()
    {
        var (empresa, cliente, colaborador) = await EscenarioAsync();
        var riesgo = await RiesgoAsync(cliente);
        await TareaAsync(empresa, "Riesgo alta", 1, colaborador, Hoy.AddDays(3), riesgo: riesgo);
        await TareaAsync(empresa, "Manual media", 2, colaborador, Hoy.AddDays(10), EstadoTarea.EnProgreso);
        await TareaAsync(empresa, "Manual baja admin", 3, empresa.AdminVinculoId, Hoy.AddDays(20), EstadoTarea.Completada);
        await TareaAsync(empresa, "Sin asignar", 2, null, null, riesgo: riesgo);

        Assert.Equal(new[] { "Manual media" }, await Titulos(cliente, "?estado=en_progreso"));
        Assert.Equal(new[] { "Riesgo alta", "Manual media" }, await Titulos(cliente, $"?responsableId={colaborador}"));
        Assert.Equal(new[] { "Sin asignar" }, await Titulos(cliente, "?sinResponsable=true"));
        Assert.Equal(new[] { "Riesgo alta", "Sin asignar" }, await Titulos(cliente, "?origen=riesgo"));
        Assert.Equal(new[] { "Manual media", "Manual baja admin" }, await Titulos(cliente, "?origen=manual"));
        Assert.Empty(await Titulos(cliente, "?origen=accidente"));
        Assert.Equal(new[] { "Manual baja admin" }, await Titulos(cliente, "?prioridad=3"));
        Assert.Equal(new[] { "Riesgo alta", "Manual media" },
            await Titulos(cliente, $"?fechaDesde={Hoy.AddDays(3):yyyy-MM-dd}&fechaHasta={Hoy.AddDays(10):yyyy-MM-dd}"));
        Assert.Equal(new[] { "Manual media" }, await Titulos(cliente, $"?origen=manual&estado=en_progreso&responsableId={colaborador}"));
    }

    [Fact]
    public async Task Las_vencidas_son_las_pendientes_o_en_progreso_con_la_fecha_pasada()
    {
        var (empresa, cliente, colaborador) = await EscenarioAsync();
        await TareaAsync(empresa, "Pendiente atrasada", 1, colaborador, Hoy.AddDays(-2));
        await TareaAsync(empresa, "En progreso atrasada", 2, colaborador, Hoy.AddDays(-1), EstadoTarea.EnProgreso);
        await TareaAsync(empresa, "Completada atrasada", 1, colaborador, Hoy.AddDays(-5), EstadoTarea.Completada);
        await TareaAsync(empresa, "Vence hoy", 1, colaborador, Hoy);
        await TareaAsync(empresa, "A tiempo", 1, colaborador, Hoy.AddDays(4));

        Assert.Equal(new[] { "Pendiente atrasada", "En progreso atrasada" }, await Titulos(cliente, "?vencidas=true"));
        Assert.Equal(new[] { "En progreso atrasada" }, await Titulos(cliente, "?vencidas=true&estado=en_progreso"));
    }

    [Theory]
    [InlineData("?vencidas=true&estado=completada")]
    [InlineData("?sinResponsable=true&responsableId=0192a000-0000-7000-8000-000000000000")]
    [InlineData("?fechaDesde=2026-12-10&fechaHasta=2026-12-01")]
    [InlineData("?prioridad=4")]
    [InlineData("?origen=otro")]
    [InlineData("?estado=terminada")]
    public async Task Rechaza_filtros_que_no_tienen_sentido(string consulta)
    {
        var (_, cliente, _) = await EscenarioAsync();

        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.GetAsync($"/api/plan-trabajo/tareas{consulta}")).StatusCode);
    }

    [Fact]
    public async Task Los_filtros_no_muestran_tareas_de_otra_empresa()
    {
        var (empresaA, _, colaboradorA) = await EscenarioAsync();
        var (_, clienteB, _) = await EscenarioAsync();
        await TareaAsync(empresaA, "De A", 1, colaboradorA, Hoy.AddDays(-1));

        Assert.Empty(await Titulos(clienteB, "?vencidas=true"));
        Assert.Empty(await Titulos(clienteB, $"?responsableId={colaboradorA}"));
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable, Guid Colaborador)> EscenarioAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Filtrar", 1);
        var colaborador = await Escenario.AgregarColaboradorAsync(empresa, $"c.{Guid.NewGuid():N}@pruebas.hn");
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"), colaborador);
    }

    private static async Task<Guid> RiesgoAsync(HttpClient cliente)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = $"Peligro {Guid.NewGuid():N}"[..20], actividad = "Planta" });
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private async Task TareaAsync(EmpresaDePrueba empresa, string titulo, short prioridad, Guid? responsable, DateOnly? fecha,
        EstadoTarea estado = EstadoTarea.Pendiente, Guid? riesgo = null)
    {
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        db.Tarea.Add(new Tarea
        {
            Titulo = titulo, Prioridad = prioridad, ResponsableId = responsable, FechaLimite = fecha, Estado = estado,
            RiesgoEmpresaId = riesgo, CreadaPorId = empresa.AdminVinculoId,
            CompletadaEn = estado == EstadoTarea.Completada ? DateTime.UtcNow : null
        });
        await db.SaveChangesAsync();
    }

    private static async Task<string[]> Titulos(HttpClient cliente, string consulta)
    {
        var r = await cliente.GetAsync($"/api/plan-trabajo/tareas{consulta}");
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).EnumerateArray().Select(t => t.GetProperty("titulo").GetString()!).ToArray();
    }
}
