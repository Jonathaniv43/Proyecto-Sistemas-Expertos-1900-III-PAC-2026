using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Eventos;

/// <summary>HU-050: medidas correctivas de un evento, que son tareas del plan con origen "accidente" (RN-23, RN-28).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class MedidasCorrectivasTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));

    [Fact]
    public async Task Cada_medida_es_una_tarea_del_plan_con_origen_accidente_y_permite_cerrar()
    {
        var (_, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);

        var r = await Medida(sst, evento, yo, Hoy.AddDays(7), "  Colocar conos  ");

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var medida = await Escenario.Json(r);
        Assert.Equal("Colocar conos", medida.GetProperty("titulo").GetString());
        Assert.Equal("pendiente", medida.GetProperty("estado").GetString());
        Assert.Equal("Persona Prueba", medida.GetProperty("responsableNombre").GetString());

        var plan = await Escenario.Json(await sst.GetAsync("/api/plan-trabajo/tareas?origen=accidente"));
        var tarea = Assert.Single(plan.EnumerateArray());
        Assert.Equal(medida.GetProperty("id").GetGuid(), tarea.GetProperty("id").GetGuid());
        Assert.Equal("accidente", tarea.GetProperty("origen").GetString());
        Assert.Equal(evento, tarea.GetProperty("accidenteId").GetGuid());

        var detalle = await Escenario.Json(await sst.GetAsync($"/api/eventos/{evento}"));
        Assert.Equal(1, detalle.GetProperty("totalMedidas").GetInt32());
        await Investigar(sst, evento, yo);
        Assert.Equal(HttpStatusCode.OK, (await sst.PostAsync($"/api/eventos/{evento}/cierre", null)).StatusCode);
    }

    [Fact]
    public async Task Desde_el_evento_se_ve_el_avance_de_sus_medidas()
    {
        var (empresa, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        var otro = await ReportarAsync(sst);
        var completada = await IdAsync(Medida(sst, evento, yo, Hoy.AddDays(2), "Capacitar al personal"));
        var cancelada = await IdAsync(Medida(sst, evento, yo, Hoy.AddDays(1), "Comprar alfombras"));
        await Medida(sst, evento, yo, Hoy.AddDays(9), "Señalizar el pasillo");
        await Medida(sst, evento, yo, Hoy.AddDays(3), "Revisar el desagüe");
        await Medida(sst, otro, yo, Hoy.AddDays(3), "De otro evento");

        await Estado(sst, completada, "en_progreso");
        await Estado(sst, completada, "completada");
        await using (var db = api.NuevoContexto(empresa.EmpresaId)) // cancelar todavía no tiene pantalla ni API
        {
            (await db.Tarea.SingleAsync(t => t.Id == cancelada)).Estado = EstadoTarea.Cancelada;
            await db.SaveChangesAsync();
        }

        var lista = (await Escenario.Json(await sst.GetAsync($"/api/eventos/{evento}/medidas-correctivas"))).EnumerateArray().ToList();
        Assert.Equal(["Revisar el desagüe", "Señalizar el pasillo", "Capacitar al personal", "Comprar alfombras"],
            lista.Select(m => m.GetProperty("titulo").GetString()));
        Assert.Equal(["pendiente", "pendiente", "completada", "cancelada"], lista.Select(m => m.GetProperty("estado").GetString()));
        Assert.NotEqual(JsonValueKind.Null, lista[2].GetProperty("completadaEn").ValueKind);
        // Las canceladas no cuentan como medida para cerrar (RN-28).
        Assert.Equal(3, (await Escenario.Json(await sst.GetAsync($"/api/eventos/{evento}"))).GetProperty("totalMedidas").GetInt32());
    }

    [Fact]
    public async Task Valida_titulo_fecha_y_responsable()
    {
        var (empresa, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        var encuestado = await Escenario.AgregarColaboradorAsync(empresa, $"e.{Guid.NewGuid():N}@pruebas.hn");
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            (await db.VinculoEmpresa.SingleAsync(v => v.Id == encuestado)).RolId = Rol.EncuestadoId;
            await db.SaveChangesAsync();
        }

        var sinTitulo = await Medida(sst, evento, yo, Hoy, " ");
        var pasada = await Medida(sst, evento, yo, Hoy.AddDays(-1));
        var deEncuestado = await Medida(sst, evento, encuestado, Hoy);
        var hoy = await Medida(sst, evento, yo, Hoy);

        Assert.Equal(HttpStatusCode.BadRequest, sinTitulo.StatusCode);
        Assert.Equal("eventos.fecha_pasada", (await Escenario.Json(pasada)).GetProperty("codigo").GetString());
        Assert.Equal("eventos.responsable_invalido", (await Escenario.Json(deEncuestado)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.Created, hoy.StatusCode);
        await using var db2 = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(1, await db2.Tarea.CountAsync(t => t.AccidenteId == evento));
    }

    [Fact]
    public async Task Un_evento_cerrado_o_descartado_ya_no_recibe_medidas()
    {
        var (_, sst, yo) = await EscenarioAsync();
        var cerrado = await ReportarAsync(sst);
        await Medida(sst, cerrado, yo, Hoy);
        await Investigar(sst, cerrado, yo);
        (await sst.PostAsync($"/api/eventos/{cerrado}/cierre", null)).EnsureSuccessStatusCode();
        var descartado = await ReportarAsync(sst);
        (await sst.PatchAsJsonAsync($"/api/eventos/{descartado}/descarte", new { })).EnsureSuccessStatusCode();

        Assert.Equal("eventos.terminado", (await Escenario.Json(await Medida(sst, cerrado, yo, Hoy))).GetProperty("codigo").GetString());
        Assert.Equal("eventos.terminado", (await Escenario.Json(await Medida(sst, descartado, yo, Hoy))).GetProperty("codigo").GetString());
        // Las medidas de un evento cerrado se siguen viendo.
        Assert.Single((await Escenario.Json(await sst.GetAsync($"/api/eventos/{cerrado}/medidas-correctivas"))).EnumerateArray());
    }

    [Fact]
    public async Task Solo_el_responsable_sst_agrega_medidas_y_solo_en_su_empresa()
    {
        var (empresa, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        await Medida(sst, evento, yo, Hoy);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var (_, sstAjeno, ajeno) = await EscenarioAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await Medida(empresa.Cliente, evento, yo, Hoy)).StatusCode); // admin: solo consulta
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.GetAsync($"/api/eventos/{evento}/medidas-correctivas")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync($"/api/eventos/{evento}/medidas-correctivas")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Medida(sstAjeno, evento, ajeno, Hoy)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await sstAjeno.GetAsync($"/api/eventos/{evento}/medidas-correctivas")).StatusCode);
        // Un responsable de otra empresa no vale, aunque el evento sea propio.
        var propio = await ReportarAsync(sstAjeno);
        Assert.Equal("eventos.responsable_invalido",
            (await Escenario.Json(await Medida(sstAjeno, propio, yo, Hoy))).GetProperty("codigo").GetString());
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(1, await db.Tarea.CountAsync(t => t.AccidenteId == evento));
    }

    // ---------- Apoyo ----------

    /// <summary>Empresa con un Responsable SST con sesión; devuelve su cliente y su vínculo.</summary>
    private async Task<(EmpresaDePrueba Empresa, HttpClient Sst, Guid Vinculo)> EscenarioAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Medidas", 1);
        var email = $"sst.{Guid.NewGuid():N}@pruebas.hn";
        var r = await empresa.Cliente.PostAsJsonAsync("/api/vinculos", new
        {
            email, primerNombre = "Persona", primerApellido = "Prueba", rolCodigo = "responsable_sst"
        });
        r.EnsureSuccessStatusCode();
        var vinculo = (await Escenario.Json(r)).GetProperty("id").GetGuid();
        await Escenario.ActivarCuentaAsync(empresa, email, "Temporal#2026");
        return (empresa, await _escenario.ClienteEmpresaAsync(email, "Temporal#2026", empresa.IdEmpresaCifrado), vinculo);
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

    private static Task<HttpResponseMessage> Medida(HttpClient cliente, Guid evento, Guid responsable, DateOnly fecha, string titulo = "Colocar conos") =>
        cliente.PostAsJsonAsync($"/api/eventos/{evento}/medidas-correctivas", new
        {
            titulo, descripcion = "En el pasillo de la bodega", prioridad = 1, responsableId = responsable,
            fechaLimite = fecha.ToString("yyyy-MM-dd"), requiereEvidencia = false
        });

    private static async Task<Guid> IdAsync(Task<HttpResponseMessage> respuesta)
    {
        var r = await respuesta;
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task Investigar(HttpClient cliente, Guid evento, Guid investigador) =>
        (await cliente.PutAsJsonAsync($"/api/eventos/{evento}/investigacion", new
        {
            causasInmediatas = "Piso mojado", causasBasicas = "Falta un procedimiento de limpieza",
            investigadaPorId = investigador, fechaInvestigacion = Hoy.ToString("yyyy-MM-dd")
        })).EnsureSuccessStatusCode();

    private static async Task Estado(HttpClient cliente, Guid tarea, string estado) =>
        (await cliente.PatchAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/estado", new { estado })).EnsureSuccessStatusCode();
}
