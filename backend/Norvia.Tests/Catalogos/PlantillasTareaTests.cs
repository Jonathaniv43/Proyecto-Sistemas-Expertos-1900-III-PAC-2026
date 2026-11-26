using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Catalogos;

/// <summary>HU-023: el staff mantiene las plantillas de tarea de cada peligro (controles sugeridos y tareas modelo).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class PlantillasTareaTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Crea_edita_y_lista_por_peligro_con_los_controles_primero()
    {
        var staff = await _escenario.ClienteStaffAsync();
        var peligro = await CrearPeligroAsync(staff);
        var otro = await CrearPeligroAsync(staff);

        await Crear(staff, peligro, "epp", "Usar guantes", 2);
        var modelo = await Escenario.Json(await Crear(staff, peligro, null, "Inspeccionar cada mes", 3, evidencia: true));
        await Crear(staff, peligro, "eliminacion", "Retirar la máquina", 1);
        await Crear(staff, otro, "ingenieria", "De otro peligro", 1);

        var lista = (await Escenario.Json(await staff.GetAsync($"/api/plantillas-tarea?peligroId={peligro}"))).EnumerateArray().ToList();

        Assert.Equal(new[] { "Retirar la máquina", "Usar guantes", "Inspeccionar cada mes" }, lista.Select(p => p.GetProperty("titulo").GetString()));
        Assert.Equal(JsonValueKind.Null, modelo.GetProperty("tipoControl").ValueKind);
        Assert.True(modelo.GetProperty("requiereEvidencia").GetBoolean());
        Assert.Equal(3, modelo.GetProperty("prioridadSugerida").GetInt32());
    }

    [Fact]
    public async Task La_edicion_es_parcial_y_puede_cambiar_entre_control_y_tarea_modelo()
    {
        var staff = await _escenario.ClienteStaffAsync();
        var peligro = await CrearPeligroAsync(staff);
        var id = (await Escenario.Json(await Crear(staff, peligro, "administrativo", "Capacitar", 2))).GetProperty("id").GetGuid();

        var r1 = await Escenario.Json(await staff.PatchAsJsonAsync($"/api/plantillas-tarea/{id}", new { titulo = "Capacitar cada año", descripcion = "Con constancia" }));
        var r2 = await Escenario.Json(await staff.PatchAsJsonAsync($"/api/plantillas-tarea/{id}", new { quitarTipoControl = true }));
        var r3 = await Escenario.Json(await staff.PatchAsJsonAsync($"/api/plantillas-tarea/{id}", new { tipoControl = "ingenieria", descripcion = "" }));

        Assert.Equal("Capacitar cada año", r1.GetProperty("titulo").GetString());
        Assert.Equal("administrativo", r1.GetProperty("tipoControl").GetString());
        Assert.Equal(JsonValueKind.Null, r2.GetProperty("tipoControl").ValueKind);
        Assert.Equal("Capacitar cada año", r2.GetProperty("titulo").GetString());
        Assert.Equal("ingenieria", r3.GetProperty("tipoControl").GetString());
        Assert.Equal(JsonValueKind.Null, r3.GetProperty("descripcion").ValueKind);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await staff.PatchAsJsonAsync($"/api/plantillas-tarea/{id}", new { tipoControl = "epp", quitarTipoControl = true })).StatusCode);
    }

    [Fact]
    public async Task Desactivada_deja_de_sugerirse_en_los_riesgos_de_ese_peligro()
    {
        var staff = await _escenario.ClienteStaffAsync();
        var peligro = await CrearPeligroAsync(staff);
        var id = (await Escenario.Json(await Crear(staff, peligro, "ingenieria", "Instalar guardas", 1))).GetProperty("id").GetGuid();
        var empresa = await _escenario.CrearEmpresaAsync("Plantillas", 1);
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");
        var riesgo = (await Escenario.Json(await sst.PostAsJsonAsync("/api/riesgos", new { peligroId = peligro, actividad = "Taller" }))).GetProperty("id").GetGuid();

        Assert.Single((await Escenario.Json(await sst.GetAsync($"/api/riesgos/{riesgo}/controles/sugeridos"))).EnumerateArray());
        var r = await staff.PatchAsJsonAsync($"/api/plantillas-tarea/{id}/estado", new { activo = false });

        Assert.False((await Escenario.Json(r)).GetProperty("activa").GetBoolean());
        Assert.Empty((await Escenario.Json(await sst.GetAsync($"/api/riesgos/{riesgo}/controles/sugeridos"))).EnumerateArray());
    }

    [Theory]
    [InlineData("", 2)]
    [InlineData("Título", 4)]
    [InlineData("Título", null)]
    public async Task Exige_titulo_y_prioridad_entre_1_y_3(string titulo, int? prioridad)
    {
        var staff = await _escenario.ClienteStaffAsync();
        var peligro = await CrearPeligroAsync(staff);

        var r = await staff.PostAsJsonAsync("/api/plantillas-tarea", new { peligroId = peligro, titulo, prioridadSugerida = prioridad });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task No_se_agrega_a_un_peligro_desactivado_ni_se_edita_una_que_no_existe()
    {
        var staff = await _escenario.ClienteStaffAsync();
        var peligro = await CrearPeligroAsync(staff);
        await staff.PatchAsJsonAsync($"/api/peligros/{peligro}/estado", new { activo = false });

        var r = await Crear(staff, peligro, "epp", "Guantes", 2);

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("catalogos.peligro_invalido", (await Escenario.Json(r)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.NotFound, (await staff.PatchAsJsonAsync($"/api/plantillas-tarea/{Guid.NewGuid()}", new { titulo = "X" })).StatusCode);
    }

    [Fact]
    public async Task Una_sesion_de_empresa_no_puede_administrarlas()
    {
        var staff = await _escenario.ClienteStaffAsync();
        var peligro = await CrearPeligroAsync(staff);
        var empresa = await _escenario.CrearEmpresaAsync("Ajena", 1);

        Assert.Equal(HttpStatusCode.Forbidden, (await empresa.Cliente.GetAsync("/api/plantillas-tarea")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Crear(empresa.Cliente, peligro, "epp", "Guantes", 2)).StatusCode);
    }

    // ---------- Apoyo ----------

    private static async Task<Guid> CrearPeligroAsync(HttpClient staff)
    {
        var r = await staff.PostAsJsonAsync("/api/peligros", new { codigo = $"p{Guid.NewGuid():N}"[..20], nombre = "Peligro de prueba", clasificacion = "mecanico" });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static Task<HttpResponseMessage> Crear(HttpClient cliente, Guid peligroId, string? tipoControl, string titulo, int prioridad, bool evidencia = false) =>
        cliente.PostAsJsonAsync("/api/plantillas-tarea",
            new { peligroId, tipoControl, titulo, prioridadSugerida = prioridad, requiereEvidencia = evidencia });
}
