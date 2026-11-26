using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Plan;

/// <summary>HU-043: asignar responsable y fecha límite, y crear tareas manuales.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class AsignarTareasTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    // Lejos de hoy, para no depender de la hora en que corran las pruebas.
    private static readonly string Futura = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)).ToString("yyyy-MM-dd");
    private static readonly string Pasada = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)).ToString("yyyy-MM-dd");

    [Fact]
    public async Task Asigna_responsable_y_fecha_y_la_tarea_sigue_pendiente()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var tarea = await TareaDelPlanAsync(cliente);
        var colaborador = await Escenario.AgregarColaboradorAsync(empresa, $"c.{Guid.NewGuid():N}@pruebas.hn");

        var r = await cliente.PutAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/asignacion", new { responsableId = colaborador, fechaLimite = Futura });

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal(colaborador, json.GetProperty("responsableId").GetGuid());
        Assert.Equal("Colaborador Prueba", json.GetProperty("responsableNombre").GetString());
        Assert.Equal(Futura, json.GetProperty("fechaLimite").GetString());
        Assert.Equal("pendiente", json.GetProperty("estado").GetString());
        Assert.Equal("riesgo", json.GetProperty("origen").GetString());
    }

    [Fact]
    public async Task Se_puede_reasignar_y_conservar_una_fecha_ya_vencida()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var tarea = await TareaDelPlanAsync(cliente);
        var otro = await Escenario.AgregarColaboradorAsync(empresa, $"c.{Guid.NewGuid():N}@pruebas.hn");
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            // Asignada hace días con una fecha que ya pasó.
            var t = await db.Tarea.SingleAsync(x => x.Id == tarea);
            t.ResponsableId = empresa.AdminVinculoId;
            t.FechaLimite = DateOnly.Parse(Pasada);
            await db.SaveChangesAsync();
        }

        var conserva = await cliente.PutAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/asignacion", new { responsableId = otro, fechaLimite = Pasada });
        var otraPasada = await cliente.PutAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/asignacion",
            new { responsableId = otro, fechaLimite = DateOnly.Parse(Pasada).AddDays(-1).ToString("yyyy-MM-dd") });

        Assert.Equal(HttpStatusCode.OK, conserva.StatusCode);
        Assert.Equal(otro, (await Escenario.Json(conserva)).GetProperty("responsableId").GetGuid());
        Assert.Equal(HttpStatusCode.BadRequest, otraPasada.StatusCode);
        Assert.Equal("plan.fecha_pasada", (await Escenario.Json(otraPasada)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Exige_responsable_y_fecha_limite_y_rechaza_una_fecha_pasada()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var tarea = await TareaDelPlanAsync(cliente);
        var url = $"/api/plan-trabajo/tareas/{tarea}/asignacion";

        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync(url, new { responsableId = empresa.AdminVinculoId })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync(url, new { fechaLimite = Futura })).StatusCode);
        var pasada = await cliente.PutAsJsonAsync(url, new { responsableId = empresa.AdminVinculoId, fechaLimite = Pasada });
        Assert.Equal("plan.fecha_pasada", (await Escenario.Json(pasada)).GetProperty("codigo").GetString());
    }

    [Theory]
    [InlineData("suspendido")]
    [InlineData("encuestado")]
    [InlineData("otra_empresa")]
    public async Task Solo_se_asigna_a_vinculos_activos_de_la_empresa_que_no_sean_encuestados(string caso)
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var tarea = await TareaDelPlanAsync(cliente);
        Guid responsable;
        if (caso == "otra_empresa")
        {
            var otra = await _escenario.CrearEmpresaAsync("Ajena", 1);
            responsable = otra.AdminVinculoId;
        }
        else
        {
            responsable = await Escenario.AgregarColaboradorAsync(empresa, $"c.{Guid.NewGuid():N}@pruebas.hn");
            await using var db = api.NuevoContexto(empresa.EmpresaId);
            var v = await db.VinculoEmpresa.SingleAsync(x => x.Id == responsable);
            if (caso == "suspendido") v.Estado = EstadoVinculo.Suspendido;
            else v.RolId = Rol.EncuestadoId;
            await db.SaveChangesAsync();
        }

        var r = await cliente.PutAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/asignacion", new { responsableId = responsable, fechaLimite = Futura });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("plan.responsable_invalido", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Una_tarea_completada_o_cancelada_no_se_reasigna()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var tarea = await TareaDelPlanAsync(cliente);
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            var t = await db.Tarea.SingleAsync(x => x.Id == tarea);
            t.Estado = EstadoTarea.Cancelada;
            await db.SaveChangesAsync();
        }

        var r = await cliente.PutAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/asignacion", new { responsableId = empresa.AdminVinculoId, fechaLimite = Futura });

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("plan.tarea_cerrada", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Crea_una_tarea_manual_ya_asignada_y_sin_origen()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();

        var r = await cliente.PostAsJsonAsync("/api/plan-trabajo/tareas", new
        {
            titulo = "  Recargar los extintores  ", descripcion = "Los de la bodega", prioridad = 2,
            responsableId = empresa.AdminVinculoId, fechaLimite = Futura, requiereEvidencia = true
        });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal("Recargar los extintores", json.GetProperty("titulo").GetString());
        Assert.Equal("manual", json.GetProperty("origen").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("riesgoId").ValueKind);
        Assert.True(json.GetProperty("requiereEvidencia").GetBoolean());
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var tarea = await db.Tarea.AsNoTracking().SingleAsync();
        Assert.Null(tarea.RiesgoEmpresaId); // RN-23: manual
        Assert.Null(tarea.AccidenteId);
        Assert.Equal(EstadoTarea.Pendiente, tarea.Estado);
        Assert.Equal(empresa.EmpresaId, tarea.EmpresaId);
    }

    [Theory]
    [InlineData("", 2, true, true)]
    [InlineData("Revisar", null, true, true)]
    [InlineData("Revisar", 4, true, true)]
    [InlineData("Revisar", 2, false, true)]
    [InlineData("Revisar", 2, true, false)]
    public async Task Una_tarea_manual_exige_titulo_prioridad_responsable_y_fecha(string titulo, int? prioridad, bool conResponsable, bool conFecha)
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();

        var r = await cliente.PostAsJsonAsync("/api/plan-trabajo/tareas", new
        {
            titulo, prioridad,
            responsableId = conResponsable ? empresa.AdminVinculoId : (Guid?)null,
            fechaLimite = conFecha ? Futura : null
        });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Los_responsables_posibles_son_los_vinculos_activos_sin_encuestados()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var colaborador = await Escenario.AgregarColaboradorAsync(empresa, $"c.{Guid.NewGuid():N}@pruebas.hn");
        var suspendido = await Escenario.AgregarColaboradorAsync(empresa, $"s.{Guid.NewGuid():N}@pruebas.hn");
        var encuestado = await Escenario.AgregarColaboradorAsync(empresa, $"e.{Guid.NewGuid():N}@pruebas.hn");
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            (await db.VinculoEmpresa.SingleAsync(v => v.Id == suspendido)).Estado = EstadoVinculo.Suspendido;
            (await db.VinculoEmpresa.SingleAsync(v => v.Id == encuestado)).RolId = Rol.EncuestadoId;
            await db.SaveChangesAsync();
        }

        var ids = (await Escenario.Json(await cliente.GetAsync("/api/plan-trabajo/responsables"))).EnumerateArray()
            .Select(x => x.GetProperty("id").GetGuid()).ToList();

        Assert.Contains(colaborador, ids);
        Assert.Contains(empresa.AdminVinculoId, ids);
        Assert.DoesNotContain(suspendido, ids);
        Assert.DoesNotContain(encuestado, ids);
        Assert.Equal(3, ids.Count); // admin, responsable SST de la sesión y el colaborador
    }

    [Fact]
    public async Task Otra_empresa_no_asigna_tareas_ajenas_y_un_colaborador_no_tiene_permiso()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var tarea = await TareaDelPlanAsync(cliente);
        var otra = await _escenario.CrearEmpresaAsync("Ajena", 1);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        var ajena = await otra.Cliente.PutAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/asignacion", new { responsableId = otra.AdminVinculoId, fechaLimite = Futura });

        Assert.Equal(HttpStatusCode.NotFound, ajena.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await colaborador.PutAsJsonAsync($"/api/plan-trabajo/tareas/{tarea}/asignacion", new { responsableId = empresa.AdminVinculoId, fechaLimite = Futura })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await colaborador.PostAsJsonAsync("/api/plan-trabajo/tareas", new { titulo = "X", prioridad = 1, responsableId = empresa.AdminVinculoId, fechaLimite = Futura })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync("/api/plan-trabajo/responsables")).StatusCode);
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Null((await db.Tarea.AsNoTracking().SingleAsync()).ResponsableId);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Asignar", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    /// <summary>Un riesgo evaluado con un control y su tarea confirmada en el plan (HU-042).</summary>
    private static async Task<Guid> TareaDelPlanAsync(HttpClient cliente)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = $"Peligro {Guid.NewGuid():N}"[..20], actividad = "Planta" });
        var riesgo = (await Escenario.Json(r)).GetProperty("id").GetGuid();
        (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/evaluaciones", new { probabilidad = 3, severidad = 4 })).EnsureSuccessStatusCode();
        (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/controles", new { tipo = "epp", descripcion = "Casco" })).EnsureSuccessStatusCode();
        var clave = (await Escenario.Json(await cliente.GetAsync("/api/plan-trabajo/propuesta"))).GetProperty("tareas")[0].GetProperty("clave").GetString();
        (await cliente.PostAsJsonAsync("/api/plan-trabajo/generacion", new { claves = new[] { clave } })).EnsureSuccessStatusCode();
        return (await Escenario.Json(await cliente.GetAsync("/api/plan-trabajo/tareas")))[0].GetProperty("id").GetGuid();
    }
}
