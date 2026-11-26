using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Plan;

/// <summary>HU-044: ver mis tareas y pasarlas por pendiente, en progreso y completada.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class AtenderTareasTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private static readonly DateOnly Futura = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));

    [Fact]
    public async Task Veo_solo_mis_tareas_con_las_abiertas_primero_por_fecha_limite()
    {
        var (empresa, (colaborador, yo)) = await EmpresaConColaboradorAsync();
        var otro = await Escenario.AgregarColaboradorAsync(empresa, $"o.{Guid.NewGuid():N}@pruebas.hn");
        await CrearTareaAsync(empresa, "Lejana", yo, Futura.AddDays(10));
        await CrearTareaAsync(empresa, "Próxima", yo, Futura);
        await CrearTareaAsync(empresa, "Hecha", yo, Futura, EstadoTarea.Completada);
        await CrearTareaAsync(empresa, "Empezada", yo, Futura.AddDays(30), EstadoTarea.EnProgreso);
        await CrearTareaAsync(empresa, "Ajena", otro, Futura);

        var titulos = (await Escenario.Json(await colaborador.GetAsync("/api/plan-trabajo/tareas/mias"))).EnumerateArray()
            .Select(t => t.GetProperty("titulo").GetString());

        Assert.Equal(new[] { "Empezada", "Próxima", "Lejana", "Hecha" }, titulos);
    }

    [Fact]
    public async Task El_responsable_la_pasa_por_en_progreso_y_completada_y_puede_reabrirla()
    {
        var (empresa, (colaborador, yo)) = await EmpresaConColaboradorAsync();
        var tarea = await CrearTareaAsync(empresa, "Señalizar", yo, Futura);

        var enProgreso = await Estado(colaborador, tarea, "en_progreso");
        var completada = await Estado(colaborador, tarea, "completada");
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
            Assert.NotNull((await db.Tarea.AsNoTracking().SingleAsync(t => t.Id == tarea)).CompletadaEn);
        var reabierta = await Estado(colaborador, tarea, "pendiente");

        Assert.Equal("en_progreso", (await Escenario.Json(enProgreso)).GetProperty("estado").GetString());
        var json = await Escenario.Json(completada);
        Assert.Equal("completada", json.GetProperty("estado").GetString());
        Assert.NotEqual(JsonValueKind.Null, json.GetProperty("completadaEn").ValueKind);
        Assert.Equal(JsonValueKind.Null, (await Escenario.Json(reabierta)).GetProperty("completadaEn").ValueKind);
        await using var db2 = api.NuevoContexto(empresa.EmpresaId);
        var final = await db2.Tarea.AsNoTracking().SingleAsync(t => t.Id == tarea);
        Assert.Equal(EstadoTarea.Pendiente, final.Estado);
        Assert.Null(final.CompletadaEn);
    }

    [Fact]
    public async Task No_puedo_cambiar_una_tarea_ajena_pero_el_responsable_sst_si()
    {
        var (empresa, (colaborador, _)) = await EmpresaConColaboradorAsync();
        var otro = await Escenario.AgregarColaboradorAsync(empresa, $"o.{Guid.NewGuid():N}@pruebas.hn");
        var tarea = await CrearTareaAsync(empresa, "De otro", otro, Futura);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var ajena = await Estado(colaborador, tarea, "en_progreso");
        var admin = await Estado(empresa.Cliente, tarea, "en_progreso"); // RN-24: el admin no es responsable ni Responsable SST
        var responsableSst = await Estado(sst, tarea, "en_progreso");

        Assert.Equal(HttpStatusCode.Forbidden, ajena.StatusCode);
        Assert.Equal("plan.tarea_ajena", (await Escenario.Json(ajena)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.Forbidden, admin.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responsableSst.StatusCode);
    }

    [Fact]
    public async Task Si_pide_evidencia_no_se_completa_sin_ella()
    {
        var (empresa, (colaborador, yo)) = await EmpresaConColaboradorAsync();
        var tarea = await CrearTareaAsync(empresa, "Con foto", yo, Futura, requiereEvidencia: true);

        var r = await Estado(colaborador, tarea, "completada");

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("plan.falta_evidencia", (await Escenario.Json(r)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.OK, (await Estado(colaborador, tarea, "en_progreso")).StatusCode);
    }

    [Fact]
    public async Task Sin_responsable_ni_fecha_no_se_inicia()
    {
        var (empresa, _) = await EmpresaConColaboradorAsync();
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");
        var tarea = await CrearTareaAsync(empresa, "Sin asignar", null, null);

        var r = await Estado(sst, tarea, "en_progreso");

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("plan.tarea_sin_asignar", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Theory]
    [InlineData("cancelada")]
    [InlineData("otro")]
    [InlineData(null)]
    public async Task Solo_acepta_pendiente_en_progreso_o_completada(string? estado)
    {
        var (empresa, (colaborador, yo)) = await EmpresaConColaboradorAsync();
        var tarea = await CrearTareaAsync(empresa, "Validar", yo, Futura);

        Assert.Equal(HttpStatusCode.BadRequest, (await Estado(colaborador, tarea, estado)).StatusCode);
    }

    [Fact]
    public async Task Una_tarea_cancelada_ya_no_cambia()
    {
        var (empresa, (colaborador, yo)) = await EmpresaConColaboradorAsync();
        var tarea = await CrearTareaAsync(empresa, "Cancelada", yo, Futura, EstadoTarea.Cancelada);

        var r = await Estado(colaborador, tarea, "en_progreso");

        Assert.Equal("plan.tarea_cancelada", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Otra_empresa_no_ve_ni_cambia_la_tarea_y_un_encuestado_no_tiene_permiso()
    {
        var (empresa, (_, yo)) = await EmpresaConColaboradorAsync();
        var tarea = await CrearTareaAsync(empresa, "Mía", yo, Futura);
        var otra = await _escenario.CrearEmpresaAsync("Ajena", 1);
        var sstAjeno = await _escenario.ClienteConRolAsync(otra, "responsable_sst");
        var encuestado = await _escenario.ClienteConRolAsync(empresa, "encuestado");

        Assert.Equal(HttpStatusCode.NotFound, (await Estado(sstAjeno, tarea, "en_progreso")).StatusCode);
        Assert.Empty((await Escenario.Json(await sstAjeno.GetAsync("/api/plan-trabajo/tareas/mias"))).EnumerateArray());
        Assert.Equal(HttpStatusCode.Forbidden, (await encuestado.GetAsync("/api/plan-trabajo/tareas/mias")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Estado(encuestado, tarea, "en_progreso")).StatusCode);
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(EstadoTarea.Pendiente, (await db.Tarea.AsNoTracking().SingleAsync()).Estado);
    }

    // ---------- Apoyo ----------

    /// <summary>Empresa con un colaborador con sesión; devuelve su cliente y su vínculo.</summary>
    private async Task<(EmpresaDePrueba Empresa, (HttpClient Cliente, Guid Vinculo) Colaborador)> EmpresaConColaboradorAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Atender", 1);
        var email = $"colaborador.{Guid.NewGuid():N}@pruebas.hn";
        var vinculo = await Escenario.AgregarColaboradorAsync(empresa, email);
        var cliente = await _escenario.ClienteEmpresaAsync(email, "Temporal#2026", empresa.IdEmpresaCifrado);
        return (empresa, (cliente, vinculo));
    }

    private async Task<Guid> CrearTareaAsync(EmpresaDePrueba empresa, string titulo, Guid? responsable, DateOnly? fecha,
        EstadoTarea estado = EstadoTarea.Pendiente, bool requiereEvidencia = false)
    {
        var tarea = new Tarea
        {
            Titulo = titulo, Prioridad = 2, ResponsableId = responsable, FechaLimite = fecha, Estado = estado,
            RequiereEvidencia = requiereEvidencia, CreadaPorId = empresa.AdminVinculoId,
            CompletadaEn = estado == EstadoTarea.Completada ? DateTime.UtcNow : null
        };
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        db.Tarea.Add(tarea);
        await db.SaveChangesAsync();
        return tarea.Id;
    }

    private static Task<HttpResponseMessage> Estado(HttpClient cliente, Guid tarea, string? estado) =>
        cliente.PatchAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/estado", new { estado });
}
