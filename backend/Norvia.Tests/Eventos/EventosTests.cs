using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Eventos;

/// <summary>
/// HU-047: reporte anónimo de eventos (accidentes, incidentes, enfermedades profesionales y condiciones inseguras),
/// validaciones de negocio, permisos por rol y consulta con filtros.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public sealed class EventosTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Reportar_evento_registra_los_cuatro_tipos_de_forma_anonima_con_datos_del_servidor()
    {
        var empresa = await _escenario.CrearEmpresaAsync("EvtA");
        var encuestado = await _escenario.ClienteConRolAsync(empresa, "encuestado");
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");
        var fecha = DateTime.UtcNow.AddMinutes(-15);

        var rCondicion = await encuestado.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "condicion_insegura",
            esGrave = true,
            fechaHoraEvento = fecha,
            descripcion = "Cableado expuesto cerca de zona húmeda."
        });
        Assert.Equal(HttpStatusCode.Created, rCondicion.StatusCode);
        var jsonCondicion = await Escenario.Json(rCondicion);
        Assert.Equal("condicion_insegura", jsonCondicion.GetProperty("tipo").GetString());
        Assert.Equal("abierto", jsonCondicion.GetProperty("estado").GetString());
        Assert.True(jsonCondicion.GetProperty("esGrave").GetBoolean());
        Assert.NotEqual(default, jsonCondicion.GetProperty("registradoEn").GetDateTime());

        var rEnfermedad = await colaborador.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "enfermedad_profesional",
            estadoEnfermedad = "sospechosa",
            esGrave = false,
            fechaHoraEvento = fecha,
            lugar = "Área de empaque",
            descripcion = "Molestia respiratoria recurrente en turno nocturno."
        });
        Assert.Equal(HttpStatusCode.Created, rEnfermedad.StatusCode);
        Assert.Equal("sospechosa", (await Escenario.Json(rEnfermedad)).GetProperty("estadoEnfermedad").GetString());

        var rAccidente = await sst.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "accidente",
            esGrave = true,
            fechaHoraEvento = fecha,
            lugar = "Bodega 1",
            descripcion = "Caída desde escalera portátil."
        });
        Assert.Equal(HttpStatusCode.Created, rAccidente.StatusCode);

        var rIncidente = await empresa.Cliente.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "incidente",
            fechaHoraEvento = fecha,
            descripcion = "Objeto cayó sin lesionar a nadie."
        });
        Assert.Equal(HttpStatusCode.Created, rIncidente.StatusCode);

        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var guardados = await db.Accidente.OrderBy(a => a.Tipo).ToListAsync();
        Assert.Equal(4, guardados.Count);
        Assert.All(guardados, a =>
        {
            Assert.Equal(empresa.EmpresaId, a.EmpresaId);
            Assert.Equal(EstadoAccidente.Abierto, a.Estado);
            Assert.Null(a.CerradoEn);
            Assert.Null(a.DescartadoPorId);
            Assert.Null(a.MotivoDescarte);
            Assert.Null(a.DescartadoEn);
        });
    }

    [Fact]
    public async Task Reportar_evento_rechaza_reglas_de_tipo_descripcion_lugar_y_fecha_futura()
    {
        var empresa = await _escenario.CrearEmpresaAsync("EvtVal");
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var ahora = DateTime.UtcNow;

        // Enfermedad profesional sin estadoEnfermedad (RN-26)
        var rSinEstadoEnf = await colaborador.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "enfermedad_profesional",
            fechaHoraEvento = ahora,
            descripcion = "Hipoacusia reportada."
        });
        Assert.Equal(HttpStatusCode.BadRequest, rSinEstadoEnf.StatusCode);

        // Otro tipo enviando estadoEnfermedad (RN-26)
        var rConEstadoEnf = await colaborador.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "condicion_insegura",
            estadoEnfermedad = "diagnosticada",
            fechaHoraEvento = ahora,
            descripcion = "Piso resbaloso."
        });
        Assert.Equal(HttpStatusCode.BadRequest, rConEstadoEnf.StatusCode);

        // Descripción vacía o solo espacios
        var rSinDesc = await colaborador.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "incidente",
            fechaHoraEvento = ahora,
            descripcion = "   "
        });
        Assert.Equal(HttpStatusCode.BadRequest, rSinDesc.StatusCode);

        // Lugar mayor a 200 caracteres
        var rLugarLargo = await colaborador.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "incidente",
            fechaHoraEvento = ahora,
            lugar = new string('A', 201),
            descripcion = "Incidente en pasillo."
        });
        Assert.Equal(HttpStatusCode.BadRequest, rLugarLargo.StatusCode);

        // Fecha y hora futura
        var rFutura = await colaborador.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "accidente",
            fechaHoraEvento = ahora.AddMinutes(30),
            descripcion = "Fecha futura."
        });
        Assert.Equal(HttpStatusCode.BadRequest, rFutura.StatusCode);
        Assert.Equal("eventos.fecha_futura", (await Escenario.Json(rFutura)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Permisos_permiten_reportar_a_todos_y_consultar_solo_a_admin_y_sst()
    {
        var empresa = await _escenario.CrearEmpresaAsync("EvtPerm");
        var encuestado = await _escenario.ClienteConRolAsync(empresa, "encuestado");
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");

        var crear = await encuestado.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "condicion_insegura",
            fechaHoraEvento = DateTime.UtcNow.AddMinutes(-5),
            descripcion = "Barandal flojo en segundo piso."
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var eventoId = (await Escenario.Json(crear)).GetProperty("id").GetGuid();

        // Encuestado y colaborador NO pueden consultar listado ni detalle.
        Assert.Equal(HttpStatusCode.Forbidden, (await encuestado.GetAsync("/api/eventos")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await encuestado.GetAsync($"/api/eventos/{eventoId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync("/api/eventos")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync($"/api/eventos/{eventoId}")).StatusCode);

        // Responsable SST y admin_empresa sí pueden consultar.
        Assert.Equal(HttpStatusCode.OK, (await sst.GetAsync("/api/eventos")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await sst.GetAsync($"/api/eventos/{eventoId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.GetAsync("/api/eventos")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.GetAsync($"/api/eventos/{eventoId}")).StatusCode);
    }

    [Fact]
    public async Task Listar_y_ver_detalle_aplican_filtros_orden_resumen_y_404()
    {
        var empresa = await _escenario.CrearEmpresaAsync("EvtList");
        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");
        var encuestado = await _escenario.ClienteConRolAsync(empresa, "encuestado");
        var descripcionLarga = new string('X', 260);

        await encuestado.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "enfermedad_profesional",
            estadoEnfermedad = "diagnosticada",
            esGrave = false,
            fechaHoraEvento = DateTime.UtcNow.AddHours(-2),
            descripcion = "Caso 1"
        });

        var rCond = await encuestado.PostAsJsonAsync("/api/eventos", new
        {
            tipo = "condicion_insegura",
            esGrave = true,
            fechaHoraEvento = DateTime.UtcNow.AddHours(-1),
            lugar = "  Bodega Norte  ",
            descripcion = descripcionLarga
        });
        var idCond = (await Escenario.Json(rCond)).GetProperty("id").GetGuid();

        var todos = (await Escenario.Json(await sst.GetAsync("/api/eventos"))).EnumerateArray().ToList();
        Assert.Equal(2, todos.Count);
        Assert.Equal(idCond, todos[0].GetProperty("id").GetGuid()); // más reciente primero
        Assert.True(todos[0].GetProperty("descripcion").GetString()!.Length <= 200);
        Assert.EndsWith("...", todos[0].GetProperty("descripcion").GetString());
        Assert.Equal("Bodega Norte", todos[0].GetProperty("lugar").GetString());

        var filtrados = (await Escenario.Json(
            await sst.GetAsync("/api/eventos?tipo=condicion_insegura&estado=abierto&esGrave=true")))
            .EnumerateArray().ToList();
        Assert.Single(filtrados);
        Assert.Equal(idCond, filtrados[0].GetProperty("id").GetGuid());

        var rFiltroInvalido = await sst.GetAsync("/api/eventos?tipo=no_existe");
        Assert.Equal(HttpStatusCode.BadRequest, rFiltroInvalido.StatusCode);
        Assert.Equal("eventos.filtro_invalido", (await Escenario.Json(rFiltroInvalido)).GetProperty("codigo").GetString());

        var detalle = await Escenario.Json(await sst.GetAsync($"/api/eventos/{idCond}"));
        Assert.Equal(descripcionLarga, detalle.GetProperty("descripcion").GetString());

        var rNoExiste = await sst.GetAsync($"/api/eventos/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, rNoExiste.StatusCode);
        Assert.Equal("eventos.no_encontrado", (await Escenario.Json(rNoExiste)).GetProperty("codigo").GetString());
    }
}
