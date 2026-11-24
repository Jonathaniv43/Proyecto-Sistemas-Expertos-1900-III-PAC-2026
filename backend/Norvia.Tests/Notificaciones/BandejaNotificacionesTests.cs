using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Notificaciones;

/// <summary>
/// HU-117: consultar mis notificaciones y marcarlas como leídas. RN-79: cada persona solo ve y marca las suyas, y solo
/// las de la empresa de su sesión. Las notificaciones salen de asignar tareas (HU-081).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class BandejaNotificacionesTests(NorviaApiFactory api)
{
    private const string PasswordTemporal = "Temporal#2026";
    private static readonly string Futura = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)).ToString("yyyy-MM-dd");

    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Muestra_mis_notificaciones_de_la_mas_reciente_a_la_mas_antigua_con_el_contador_de_no_leidas()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Bandeja");
        var (colaborador, cliente) = await ColaboradorAsync(empresa);
        await AsignarTareaAsync(empresa, colaborador, "Primera tarea");
        await AsignarTareaAsync(empresa, colaborador, "Segunda tarea");

        var r = await cliente.GetAsync("/api/notificaciones");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal(2, json.GetProperty("noLeidas").GetInt32());
        var lista = json.GetProperty("notificaciones").EnumerateArray().ToList();
        Assert.Equal(2, lista.Count);
        Assert.StartsWith("Segunda tarea.", lista[0].GetProperty("mensaje").GetString());
        Assert.StartsWith("Primera tarea.", lista[1].GetProperty("mensaje").GetString());
        Assert.All(lista, n =>
        {
            Assert.Equal("tarea_asignada", n.GetProperty("tipo").GetString());
            Assert.Equal("Te asignaron una tarea", n.GetProperty("titulo").GetString());
            Assert.Equal(System.Text.Json.JsonValueKind.Null, n.GetProperty("leidaEn").ValueKind);
            Assert.NotEqual(Guid.Empty, n.GetProperty("tareaId").GetGuid());
        });

        var contador = await Escenario.Json(await cliente.GetAsync("/api/notificaciones/no-leidas"));
        Assert.Equal(2, contador.GetProperty("noLeidas").GetInt32());
    }

    [Fact]
    public async Task Marcar_como_leida_baja_el_contador_y_repetirlo_no_cambia_la_hora_de_lectura()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Lectura");
        var (colaborador, cliente) = await ColaboradorAsync(empresa);
        await AsignarTareaAsync(empresa, colaborador, "Revisar extintores");
        var id = await PrimeraNotificacionAsync(cliente);

        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PatchAsync($"/api/notificaciones/{id}/lectura", null)).StatusCode);
        var primera = await LeidaEnAsync(empresa.EmpresaId, id);
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PatchAsync($"/api/notificaciones/{id}/lectura", null)).StatusCode);

        Assert.NotNull(primera);
        Assert.Equal(primera, await LeidaEnAsync(empresa.EmpresaId, id));
        Assert.Equal(0, (await Escenario.Json(await cliente.GetAsync("/api/notificaciones/no-leidas"))).GetProperty("noLeidas").GetInt32());
        var lista = await Escenario.Json(await cliente.GetAsync("/api/notificaciones"));
        Assert.Equal(0, lista.GetProperty("noLeidas").GetInt32());
        Assert.NotEqual(System.Text.Json.JsonValueKind.Null,
            lista.GetProperty("notificaciones")[0].GetProperty("leidaEn").ValueKind);
    }

    [Fact]
    public async Task Otra_persona_de_la_misma_empresa_no_ve_ni_marca_mis_notificaciones()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Ajena");
        var (yo, miCliente) = await ColaboradorAsync(empresa);
        var (_, otroCliente) = await ColaboradorAsync(empresa);
        await AsignarTareaAsync(empresa, yo, "Tarea solo mía");
        var mia = await PrimeraNotificacionAsync(miCliente);

        var lista = await Escenario.Json(await otroCliente.GetAsync("/api/notificaciones"));
        var marcar = await otroCliente.PatchAsync($"/api/notificaciones/{mia}/lectura", null);

        Assert.Empty(lista.GetProperty("notificaciones").EnumerateArray());
        Assert.Equal(0, lista.GetProperty("noLeidas").GetInt32());
        Assert.Equal(HttpStatusCode.NotFound, marcar.StatusCode);
        Assert.Equal("notificaciones.no_encontrada", (await Escenario.Json(marcar)).GetProperty("codigo").GetString());
        Assert.Null(await LeidaEnAsync(empresa.EmpresaId, mia));
        // Ni el administrador de la empresa ve las notificaciones de otra persona.
        Assert.Empty((await Escenario.Json(await empresa.Cliente.GetAsync("/api/notificaciones")))
            .GetProperty("notificaciones").EnumerateArray());
    }

    [Fact]
    public async Task Una_sesion_de_otra_empresa_no_ve_ni_marca_notificaciones_ajenas()
    {
        var a = await _escenario.CrearEmpresaAsync("Empresa A");
        var b = await _escenario.CrearEmpresaAsync("Empresa B");
        var (colaboradorA, clienteA) = await ColaboradorAsync(a);
        await AsignarTareaAsync(a, colaboradorA, "Tarea de A");
        var deA = await PrimeraNotificacionAsync(clienteA);

        var marcar = await b.Cliente.PatchAsync($"/api/notificaciones/{deA}/lectura", null);

        Assert.Equal(HttpStatusCode.NotFound, marcar.StatusCode);
        Assert.Null(await LeidaEnAsync(a.EmpresaId, deA));
        Assert.Empty((await Escenario.Json(await b.Cliente.GetAsync("/api/notificaciones"))).GetProperty("notificaciones").EnumerateArray());
    }

    [Fact]
    public async Task La_misma_persona_en_dos_empresas_solo_ve_las_de_la_empresa_de_su_sesion()
    {
        // Mismo correo (mismo Acceso) con un vínculo en cada empresa: cada login es por empresa (HU-019).
        var a = await _escenario.CrearEmpresaAsync("Dos empresas A");
        var b = await _escenario.CrearEmpresaAsync("Dos empresas B");
        var email = $"doble.{Guid.NewGuid():N}@pruebas.hn";
        var enA = await Escenario.AgregarColaboradorAsync(a, email);
        var enB = await Escenario.AgregarColaboradorAsync(b, email);
        await AsignarTareaAsync(a, enA, "Tarea en A");
        await AsignarTareaAsync(b, enB, "Tarea en B");

        var sesionA = await _escenario.ClienteEmpresaAsync(email, PasswordTemporal, a.IdEmpresaCifrado);
        var lista = await Escenario.Json(await sesionA.GetAsync("/api/notificaciones"));

        var mensajes = lista.GetProperty("notificaciones").EnumerateArray().Select(n => n.GetProperty("mensaje").GetString()).ToList();
        Assert.Single(mensajes);
        Assert.StartsWith("Tarea en A.", mensajes[0]);
        Assert.Equal(1, lista.GetProperty("noLeidas").GetInt32());
    }

    [Theory]
    [InlineData("admin_empresa")]
    [InlineData("responsable_sst")]
    [InlineData("colaborador")]
    [InlineData("encuestado")]
    public async Task Todos_los_roles_de_empresa_consultan_su_bandeja(string rol)
    {
        var empresa = await _escenario.CrearEmpresaAsync($"Rol {rol}");
        var cliente = rol == "admin_empresa" ? empresa.Cliente : await _escenario.ClienteConRolAsync(empresa, rol);

        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/notificaciones")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/notificaciones/no-leidas")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.PatchAsync($"/api/notificaciones/{Guid.NewGuid()}/lectura", null)).StatusCode);
    }

    [Fact]
    public async Task El_staff_de_plataforma_y_las_peticiones_sin_sesion_no_tienen_bandeja()
    {
        var staff = await _escenario.ClienteStaffAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await staff.GetAsync("/api/notificaciones")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.CreateClient().GetAsync("/api/notificaciones")).StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Rechaza_un_limite_fuera_de_rango(int limite)
    {
        var empresa = await _escenario.CrearEmpresaAsync("Límite");

        Assert.Equal(HttpStatusCode.BadRequest, (await empresa.Cliente.GetAsync($"/api/notificaciones?limite={limite}")).StatusCode);
    }

    private async Task<(Guid Vinculo, HttpClient Cliente)> ColaboradorAsync(EmpresaDePrueba empresa)
    {
        var email = $"c.{Guid.NewGuid():N}@pruebas.hn";
        var vinculo = await Escenario.AgregarColaboradorAsync(empresa, email);
        return (vinculo, await _escenario.ClienteEmpresaAsync(email, PasswordTemporal, empresa.IdEmpresaCifrado));
    }

    /// <summary>El administrador le crea una tarea manual ya asignada (HU-043), que la notifica (HU-081).</summary>
    private static async Task AsignarTareaAsync(EmpresaDePrueba empresa, Guid responsable, string titulo)
    {
        var r = await empresa.Cliente.PostAsJsonAsync("/api/plan-trabajo/tareas",
            new { titulo, prioridad = 2, responsableId = responsable, fechaLimite = Futura });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    private static async Task<Guid> PrimeraNotificacionAsync(HttpClient cliente) =>
        (await Escenario.Json(await cliente.GetAsync("/api/notificaciones"))).GetProperty("notificaciones")[0].GetProperty("id").GetGuid();

    private async Task<DateTime?> LeidaEnAsync(Guid empresaId, Guid notificacionId)
    {
        await using var db = api.NuevoContexto(empresaId);
        return await db.Notificacion.AsNoTracking().Where(n => n.Id == notificacionId).Select(n => n.LeidaEn).SingleAsync();
    }
}
