using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Eventos;

/// <summary>HU-049: investigar, cerrar y descartar eventos (RN-28).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class InvestigarEventosTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));

    [Fact]
    public async Task Registrar_la_investigacion_la_deja_en_investigacion_y_corregirla_no_la_duplica()
    {
        var (empresa, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        var detalle = await Escenario.Json(await sst.GetAsync($"/api/eventos/{evento}"));
        Assert.Equal(JsonValueKind.Null, detalle.GetProperty("investigacion").ValueKind);
        Assert.Equal(0, detalle.GetProperty("totalMedidas").GetInt32());

        var r = await Investigar(sst, evento, yo, Hoy.AddDays(-1), "Piso mojado sin señalizar");
        var r2 = await Investigar(sst, evento, yo, Hoy, "Piso mojado y sin conos");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Escenario.Json(r2);
        Assert.Equal("en_investigacion", json.GetProperty("estado").GetString());
        var inv = json.GetProperty("investigacion");
        Assert.Equal("Piso mojado y sin conos", inv.GetProperty("causasInmediatas").GetString());
        Assert.Equal("Falta un procedimiento de limpieza", inv.GetProperty("causasBasicas").GetString());
        Assert.Equal("Persona Prueba", inv.GetProperty("investigadaPorNombre").GetString());
        Assert.Equal(Hoy.ToString("yyyy-MM-dd"), inv.GetProperty("fechaInvestigacion").GetString());
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(1, await db.InvestigacionAccidente.CountAsync(i => i.AccidenteId == evento));
    }

    [Fact]
    public async Task Valida_causas_fecha_e_investigador()
    {
        var (empresa, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        var encuestado = await Escenario.AgregarColaboradorAsync(empresa, $"e.{Guid.NewGuid():N}@pruebas.hn");
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            (await db.VinculoEmpresa.SingleAsync(v => v.Id == encuestado)).RolId = Rol.EncuestadoId;
            await db.SaveChangesAsync();
        }

        var sinCausas = await sst.PutAsJsonAsync($"/api/eventos/{evento}/investigacion",
            new { causasInmediatas = " ", causasBasicas = "", investigadaPorId = yo, fechaInvestigacion = Hoy.ToString("yyyy-MM-dd") });
        var futura = await Investigar(sst, evento, yo, Hoy.AddDays(1));
        var antesDelEvento = await Investigar(sst, evento, yo, Hoy.AddDays(-10));
        var deEncuestado = await Investigar(sst, evento, encuestado, Hoy);

        Assert.Equal(HttpStatusCode.BadRequest, sinCausas.StatusCode);
        Assert.Equal("eventos.fecha_investigacion_invalida", (await Escenario.Json(futura)).GetProperty("codigo").GetString());
        Assert.Equal("eventos.fecha_investigacion_invalida", (await Escenario.Json(antesDelEvento)).GetProperty("codigo").GetString());
        Assert.Equal("eventos.investigador_invalido", (await Escenario.Json(deEncuestado)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task No_se_cierra_sin_investigacion_ni_sin_medidas_y_con_ambas_si()
    {
        var (empresa, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);

        var sinInvestigacion = await sst.PostAsync($"/api/eventos/{evento}/cierre", null);
        await Investigar(sst, evento, yo, Hoy);
        var sinMedidas = await sst.PostAsync($"/api/eventos/{evento}/cierre", null);
        await MedidaAsync(empresa, evento, EstadoTarea.Cancelada); // una cancelada no cuenta
        var conCancelada = await sst.PostAsync($"/api/eventos/{evento}/cierre", null);
        await MedidaAsync(empresa, evento, EstadoTarea.Pendiente);
        var cerrado = await sst.PostAsync($"/api/eventos/{evento}/cierre", null);

        Assert.Equal("eventos.sin_investigacion", (await Escenario.Json(sinInvestigacion)).GetProperty("codigo").GetString());
        Assert.Equal("eventos.sin_medidas", (await Escenario.Json(sinMedidas)).GetProperty("codigo").GetString());
        Assert.Equal("eventos.sin_medidas", (await Escenario.Json(conCancelada)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.OK, cerrado.StatusCode);
        var json = await Escenario.Json(cerrado);
        Assert.Equal("cerrado", json.GetProperty("estado").GetString());
        Assert.NotEqual(JsonValueKind.Null, json.GetProperty("cerradoEn").ValueKind);
        Assert.Equal(1, json.GetProperty("totalMedidas").GetInt32());
        Assert.Equal("eventos.terminado", (await Escenario.Json(await Investigar(sst, evento, yo, Hoy))).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Descartar_guarda_quien_cuando_y_por_que_y_ya_no_se_investiga()
    {
        var (_, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        var sinMotivo = await ReportarAsync(sst);

        var r = await sst.PatchAsJsonAsync($"/api/eventos/{evento}/descarte", new { motivo = "  Reporte duplicado  " });
        var r2 = await sst.PatchAsJsonAsync($"/api/eventos/{sinMotivo}/descarte", new { });

        var json = await Escenario.Json(r);
        Assert.Equal("descartado", json.GetProperty("estado").GetString());
        Assert.Equal("Reporte duplicado", json.GetProperty("motivoDescarte").GetString());
        Assert.Equal("Persona Prueba", json.GetProperty("descartadoPorNombre").GetString());
        Assert.NotEqual(JsonValueKind.Null, json.GetProperty("descartadoEn").ValueKind);
        Assert.Equal(JsonValueKind.Null, (await Escenario.Json(r2)).GetProperty("motivoDescarte").ValueKind);
        Assert.Equal("eventos.terminado", (await Escenario.Json(await Investigar(sst, evento, yo, Hoy))).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await sst.PatchAsJsonAsync($"/api/eventos/{evento}/descarte", new { })).StatusCode);
    }

    [Fact]
    public async Task Solo_el_responsable_sst_de_la_empresa_investiga_cierra_o_descarta()
    {
        var (empresa, sst, yo) = await EscenarioAsync();
        var evento = await ReportarAsync(sst);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var (_, sstAjeno, _) = await EscenarioAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await Investigar(empresa.Cliente, evento, yo, Hoy)).StatusCode); // admin: solo consulta
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.PatchAsJsonAsync($"/api/eventos/{evento}/descarte", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Investigar(sstAjeno, evento, yo, Hoy)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await sstAjeno.PostAsync($"/api/eventos/{evento}/cierre", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await sstAjeno.PatchAsJsonAsync($"/api/eventos/{evento}/descarte", new { })).StatusCode);
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(EstadoAccidente.Abierto, (await db.Accidente.AsNoTracking().SingleAsync(a => a.Id == evento)).Estado);
    }

    // ---------- Apoyo ----------

    /// <summary>Empresa con un Responsable SST con sesión; devuelve su cliente y su vínculo.</summary>
    private async Task<(EmpresaDePrueba Empresa, HttpClient Sst, Guid Vinculo)> EscenarioAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Investigar", 1);
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

    /// <summary>Un accidente de hace 3 días, reportado por la API.</summary>
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

    private static Task<HttpResponseMessage> Investigar(HttpClient cliente, Guid evento, Guid investigador, DateOnly fecha, string causas = "Piso mojado") =>
        cliente.PutAsJsonAsync($"/api/eventos/{evento}/investigacion", new
        {
            causasInmediatas = causas, causasBasicas = "Falta un procedimiento de limpieza", conclusiones = "Capacitar al personal",
            investigadaPorId = investigador, fechaInvestigacion = fecha.ToString("yyyy-MM-dd")
        });

    /// <summary>Una medida correctiva: tarea con origen en el evento (HU-050).</summary>
    private async Task MedidaAsync(EmpresaDePrueba empresa, Guid evento, EstadoTarea estado)
    {
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        db.Tarea.Add(new Tarea { Titulo = "Colocar conos", Prioridad = 1, AccidenteId = evento, Estado = estado, CreadaPorId = empresa.AdminVinculoId });
        await db.SaveChangesAsync();
    }
}
