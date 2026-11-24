using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Plan;

/// <summary>HU-042: generar el plan de trabajo a partir de los riesgos y sus controles.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class GenerarPlanTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Propone_una_tarea_por_control_y_por_tarea_modelo_del_peligro_sin_guardar_nada()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        await CrearPlantillaAsync(peligro, null, "Inspeccionar el área cada mes", prioridad: 2, evidencia: true);
        var sugerida = await CrearPlantillaAsync(peligro, TipoControl.Ingenieria, "Instalar barandas", evidencia: true);
        await CrearPlantillaAsync(peligro, TipoControl.Epp, "Usar arnés"); // sugerencia no aceptada: no entra
        await CrearPlantillaAsync(peligro, null, "Plantilla inactiva", activa: false);
        var riesgo = await AgregarDelCatalogoAsync(cliente, peligro, 3, 4);
        await Control(cliente, riesgo, "epp", "Casco");
        await Control(cliente, riesgo, "ingenieria", "Instalar barandas de 1 m", sugerida);

        var propuesta = await Propuesta(cliente);
        var tareas = propuesta.GetProperty("tareas").EnumerateArray().ToList();

        Assert.Equal(new[] { "Instalar barandas", "Casco", "Inspeccionar el área cada mes" }, tareas.Select(t => t.GetProperty("titulo").GetString()));
        Assert.Equal(new[] { "control", "control", "plantilla" }, tareas.Select(t => t.GetProperty("origen").GetString()));
        Assert.All(tareas, t => Assert.Equal(1, t.GetProperty("prioridad").GetInt32())); // alto → 1
        Assert.True(tareas[0].GetProperty("requiereEvidencia").GetBoolean());
        Assert.Equal("Instalar barandas de 1 m", tareas[0].GetProperty("descripcion").GetString());
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.False(await db.Tarea.AnyAsync());
    }

    [Fact]
    public async Task Ordena_por_el_nivel_del_riesgo_y_deja_fuera_los_sin_evaluar()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var bajo = await AgregarDescritoAsync(cliente, "Bajo", 1, 2);
        var critico = await AgregarDescritoAsync(cliente, "Crítico", 5, 4);
        var medio = await AgregarDescritoAsync(cliente, "Medio", 2, 3);
        var sinEvaluar = await AgregarDescritoAsync(cliente, "Sin evaluar");
        foreach (var r in new[] { bajo, critico, medio, sinEvaluar }) await Control(cliente, r, "administrativo", "Capacitar");

        var propuesta = await Propuesta(cliente);
        var tareas = propuesta.GetProperty("tareas").EnumerateArray().ToList();

        Assert.Equal(new[] { "Crítico", "Medio", "Bajo" }, tareas.Select(t => t.GetProperty("riesgoNombre").GetString()));
        Assert.Equal(new[] { 1, 2, 3 }, tareas.Select(t => t.GetProperty("prioridad").GetInt32()));
        Assert.Equal(1, propuesta.GetProperty("riesgosSinEvaluar").GetInt32());
    }

    [Fact]
    public async Task Confirmar_crea_solo_las_elegidas_con_su_origen_y_las_desmarcadas_se_vuelven_a_proponer()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarDescritoAsync(cliente, "Escalera", 3, 4);
        await Control(cliente, riesgo, "sustitucion", "Escalera de fibra");
        await Control(cliente, riesgo, "epp", "Casco");
        var claves = (await Propuesta(cliente)).GetProperty("tareas").EnumerateArray().Select(t => t.GetProperty("clave").GetString()!).ToList();

        var r = await cliente.PostAsJsonAsync("/api/plan-trabajo/generacion", new { claves = new[] { claves[0] } });

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(1, (await Escenario.Json(r)).GetProperty("tareasCreadas").GetInt32());
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            var tarea = await db.Tarea.AsNoTracking().SingleAsync();
            Assert.Equal(riesgo, tarea.RiesgoEmpresaId); // RN-23
            Assert.NotNull(tarea.ControlId);
            Assert.Equal(EstadoTarea.Pendiente, tarea.Estado);
            Assert.Null(tarea.ResponsableId);
            Assert.Equal(empresa.EmpresaId, tarea.EmpresaId);
        }
        var resto = (await Propuesta(cliente)).GetProperty("tareas").EnumerateArray().Select(t => t.GetProperty("titulo").GetString()).ToList();
        Assert.Equal(new[] { "Casco" }, resto);
    }

    [Fact]
    public async Task Generarlo_dos_veces_no_duplica_tareas()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        await CrearPlantillaAsync(peligro, null, "Revisar extintores");
        var riesgo = await AgregarDelCatalogoAsync(cliente, peligro, 2, 2);
        await Control(cliente, riesgo, "ingenieria", "Ventilación");
        var claves = (await Propuesta(cliente)).GetProperty("tareas").EnumerateArray().Select(t => t.GetProperty("clave").GetString()!).ToArray();

        var primera = await Escenario.Json(await cliente.PostAsJsonAsync("/api/plan-trabajo/generacion", new { claves }));
        var segunda = await Escenario.Json(await cliente.PostAsJsonAsync("/api/plan-trabajo/generacion", new { claves }));

        Assert.Equal(2, primera.GetProperty("tareasCreadas").GetInt32());
        Assert.Equal(0, segunda.GetProperty("tareasCreadas").GetInt32());
        Assert.Empty((await Propuesta(cliente)).GetProperty("tareas").EnumerateArray());
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        Assert.Equal(2, await db.Tarea.CountAsync());
    }

    [Fact]
    public async Task Un_control_nuevo_despues_de_generar_entra_en_la_siguiente_propuesta()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarDescritoAsync(cliente, "Ruido", 3, 3);
        await Control(cliente, riesgo, "ingenieria", "Encerrar la máquina");
        await Confirmar(cliente);
        await Control(cliente, riesgo, "epp", "Protectores auditivos");

        var titulos = (await Propuesta(cliente)).GetProperty("tareas").EnumerateArray().Select(t => t.GetProperty("titulo").GetString());

        Assert.Equal(new[] { "Protectores auditivos" }, titulos);
    }

    [Fact]
    public async Task La_lista_del_plan_va_por_prioridad_y_nivel_con_su_riesgo_y_control()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var medio = await AgregarDescritoAsync(cliente, "Medio", 2, 3);
        var alto = await AgregarDescritoAsync(cliente, "Alto", 2, 5);
        var critico = await AgregarDescritoAsync(cliente, "Crítico", 4, 5);
        foreach (var r in new[] { medio, alto, critico }) await Control(cliente, r, "administrativo", $"Control {r:N}"[..16]);
        await Confirmar(cliente);

        var tareas = (await Escenario.Json(await cliente.GetAsync("/api/plan-trabajo/tareas"))).EnumerateArray().ToList();

        Assert.Equal(new[] { "Crítico", "Alto", "Medio" }, tareas.Select(t => t.GetProperty("riesgoNombre").GetString()));
        Assert.Equal("riesgo", tareas[0].GetProperty("origen").GetString());
        Assert.Equal("critico", tareas[0].GetProperty("riesgoClasificacion").GetString());
        Assert.Equal("administrativo", tareas[0].GetProperty("controlTipo").GetString());
        Assert.Equal("pendiente", tareas[0].GetProperty("estado").GetString());
        Assert.Equal(JsonValueKind.Null, tareas[0].GetProperty("responsableId").ValueKind);
    }

    [Fact]
    public async Task Dentro_de_un_riesgo_la_lista_va_por_efectividad_del_control_y_luego_las_tareas_modelo()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        await CrearPlantillaAsync(peligro, null, "Auditar el área");
        var riesgo = await AgregarDelCatalogoAsync(cliente, peligro, 3, 4);
        await Control(cliente, riesgo, "epp", "Guantes");
        await Control(cliente, riesgo, "eliminacion", "Retirar la máquina");
        await Control(cliente, riesgo, "administrativo", "Capacitar");
        await Confirmar(cliente); // las cuatro se crean juntas, con la misma fecha

        var titulos = (await Escenario.Json(await cliente.GetAsync("/api/plan-trabajo/tareas"))).EnumerateArray()
            .Select(t => t.GetProperty("titulo").GetString());

        Assert.Equal(new[] { "Retirar la máquina", "Capacitar", "Guantes", "Auditar el área" }, titulos);
    }

    [Fact]
    public async Task Rechaza_confirmar_sin_tareas_elegidas()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();

        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync("/api/plan-trabajo/generacion", new { claves = Array.Empty<string>() })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PostAsJsonAsync("/api/plan-trabajo/generacion", new { })).StatusCode);
    }

    [Fact]
    public async Task Una_empresa_no_ve_ni_genera_con_los_riesgos_de_otra()
    {
        var (empresaA, clienteA) = await EmpresaConResponsableAsync();
        var (empresaB, clienteB) = await EmpresaConResponsableAsync();
        var riesgoA = await AgregarDescritoAsync(clienteA, "De A", 3, 4);
        await Control(clienteA, riesgoA, "epp", "Guantes");
        var claveA = (await Propuesta(clienteA)).GetProperty("tareas")[0].GetProperty("clave").GetString();

        Assert.Empty((await Propuesta(clienteB)).GetProperty("tareas").EnumerateArray());
        var r = await clienteB.PostAsJsonAsync("/api/plan-trabajo/generacion", new { claves = new[] { claveA } });
        Assert.Equal(0, (await Escenario.Json(r)).GetProperty("tareasCreadas").GetInt32());
        await Confirmar(clienteA);
        Assert.Empty((await Escenario.Json(await clienteB.GetAsync("/api/plan-trabajo/tareas"))).EnumerateArray());
        await using var db = api.NuevoContexto(empresaB.EmpresaId);
        Assert.False(await db.Tarea.AnyAsync());
        await using var dbA = api.NuevoContexto(empresaA.EmpresaId);
        Assert.Equal(1, await dbA.Tarea.CountAsync());
    }

    [Fact]
    public async Task Un_colaborador_no_genera_el_plan()
    {
        var (empresa, _) = await EmpresaConResponsableAsync();
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync("/api/plan-trabajo/propuesta")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.PostAsJsonAsync("/api/plan-trabajo/generacion", new { claves = new[] { "x" } })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync("/api/plan-trabajo/tareas")).StatusCode);
    }

    [Fact]
    public async Task La_base_rechaza_dos_tareas_para_el_mismo_control()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarDescritoAsync(cliente, "Doble", 3, 4);
        await Control(cliente, riesgo, "epp", "Casco");
        await Confirmar(cliente);

        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var existente = await db.Tarea.AsNoTracking().SingleAsync();
        db.Tarea.Add(new Domain.Entities.Plan.Tarea
        {
            Titulo = "Otra", Prioridad = 1, RiesgoEmpresaId = riesgo, ControlId = existente.ControlId, CreadaPorId = existente.CreadaPorId
        });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Plan", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    private async Task<Guid> CrearPeligroAsync()
    {
        var peligro = new Peligro { Codigo = $"p{Guid.NewGuid():N}"[..20], Nombre = $"Peligro {Guid.NewGuid():N}"[..20], Clasificacion = ClasificacionPeligro.Locativo };
        await using var db = api.NuevoContexto(empresaId: null);
        db.Peligro.Add(peligro);
        await db.SaveChangesAsync();
        return peligro.Id;
    }

    private async Task<Guid> CrearPlantillaAsync(Guid peligroId, TipoControl? tipo, string titulo, short prioridad = 1, bool evidencia = false, bool activa = true)
    {
        var p = new PlantillaTarea { PeligroId = peligroId, TipoControl = tipo, Titulo = titulo, PrioridadSugerida = prioridad, RequiereEvidencia = evidencia, Activa = activa };
        await using var db = api.NuevoContexto(empresaId: null);
        db.PlantillaTarea.Add(p);
        await db.SaveChangesAsync();
        return p.Id;
    }

    private static async Task<Guid> Crear(HttpClient cliente, object cuerpo, int? probabilidad, int? severidad)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", cuerpo);
        r.EnsureSuccessStatusCode();
        var id = (await Escenario.Json(r)).GetProperty("id").GetGuid();
        if (probabilidad is not null)
            (await cliente.PostAsJsonAsync($"/api/riesgos/{id}/evaluaciones", new { probabilidad, severidad })).EnsureSuccessStatusCode();
        return id;
    }

    private static Task<Guid> AgregarDelCatalogoAsync(HttpClient cliente, Guid peligroId, int probabilidad, int severidad) =>
        Crear(cliente, new { peligroId, actividad = "Taller" }, probabilidad, severidad);

    private static Task<Guid> AgregarDescritoAsync(HttpClient cliente, string descripcion, int? probabilidad = null, int? severidad = null) =>
        Crear(cliente, new { descripcionPeligro = descripcion, actividad = "Planta" }, probabilidad, severidad);

    private static async Task Control(HttpClient cliente, Guid riesgoId, string tipo, string descripcion, Guid? plantillaId = null) =>
        (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/controles", new { tipo, descripcion, plantillaId })).EnsureSuccessStatusCode();

    private static async Task<JsonElement> Propuesta(HttpClient cliente) => await Escenario.Json(await cliente.GetAsync("/api/plan-trabajo/propuesta"));

    private static async Task Confirmar(HttpClient cliente)
    {
        var claves = (await Propuesta(cliente)).GetProperty("tareas").EnumerateArray().Select(t => t.GetProperty("clave").GetString()!).ToArray();
        (await cliente.PostAsJsonAsync("/api/plan-trabajo/generacion", new { claves })).EnsureSuccessStatusCode();
    }
}
