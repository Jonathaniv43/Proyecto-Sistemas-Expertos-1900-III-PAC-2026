using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Riesgos;

/// <summary>HU-039: controles sugeridos por las plantillas del peligro, controles propios y riesgos altos sin control.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class ControlesRiesgoTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Sugiere_las_plantillas_activas_del_peligro_de_mayor_a_menor_efectividad()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        await CrearPlantillaAsync(peligro, TipoControl.Epp, "Usar guantes");
        await CrearPlantillaAsync(peligro, TipoControl.Eliminacion, "Retirar la máquina");
        await CrearPlantillaAsync(peligro, TipoControl.Administrativo, "Capacitar");
        await CrearPlantillaAsync(peligro, TipoControl.Ingenieria, "Inactiva", activa: false);
        await CrearPlantillaAsync(peligro, null, "Tarea sin tipo de control");
        await CrearPlantillaAsync(await CrearPeligroAsync(), TipoControl.Sustitucion, "De otro peligro");
        var riesgo = await AgregarDelCatalogoAsync(cliente, peligro);

        var sugeridos = (await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}/controles/sugeridos"))).EnumerateArray().ToList();

        Assert.Equal(new[] { "eliminacion", "administrativo", "epp" }, sugeridos.Select(s => s.GetProperty("tipo").GetString()));
        Assert.Equal("Retirar la máquina", sugeridos[0].GetProperty("titulo").GetString());
    }

    [Fact]
    public async Task Ninguna_sugerencia_se_guarda_hasta_aceptarla_y_aceptada_deja_de_sugerirse()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        var plantilla = await CrearPlantillaAsync(peligro, TipoControl.Ingenieria, "Instalar guarda");
        var riesgo = await AgregarDelCatalogoAsync(cliente, peligro);

        await cliente.GetAsync($"/api/riesgos/{riesgo}/controles/sugeridos");
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
            Assert.False(await db.Control.AnyAsync(c => c.RiesgoEmpresaId == riesgo)); // RN-76

        // Se acepta editada: otra descripción y otro tipo.
        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/controles",
            new { tipo = "administrativo", descripcion = "  Instalar guarda y señalizar  ", plantillaId = plantilla });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var control = await Escenario.Json(r);
        Assert.Equal("administrativo", control.GetProperty("tipo").GetString());
        Assert.Equal("Instalar guarda y señalizar", control.GetProperty("descripcion").GetString());
        Assert.Equal("propuesto", control.GetProperty("estado").GetString());
        Assert.Equal(plantilla, control.GetProperty("plantillaId").GetGuid());
        Assert.Empty((await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}/controles/sugeridos"))).EnumerateArray());

        var otraVez = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/controles",
            new { tipo = "ingenieria", descripcion = "Instalar guarda", plantillaId = plantilla });
        Assert.Equal(HttpStatusCode.Conflict, otraVez.StatusCode);
        Assert.Equal("riesgos.sugerencia_aceptada", (await Escenario.Json(otraVez)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Agrega_un_control_propio_y_los_lista_del_mas_efectivo_al_menos_efectivo()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarDescritoAsync(cliente);

        await Crear(cliente, riesgo, "epp", "Casco");
        await Crear(cliente, riesgo, "sustitucion", "Escalera de fibra");
        var controles = (await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}/controles"))).EnumerateArray().ToList();

        Assert.Equal(new[] { "sustitucion", "epp" }, controles.Select(c => c.GetProperty("tipo").GetString()));
        Assert.Equal(System.Text.Json.JsonValueKind.Null, controles[0].GetProperty("plantillaId").ValueKind);
        Assert.Equal(2, (await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}"))).GetProperty("totalControles").GetInt32());
        // Un peligro descrito por la empresa no tiene plantillas.
        Assert.Empty((await Escenario.Json(await cliente.GetAsync($"/api/riesgos/{riesgo}/controles/sugeridos"))).EnumerateArray());
    }

    [Theory]
    [InlineData(null, "Casco")]
    [InlineData("otro", "Casco")]
    [InlineData("epp", "   ")]
    [InlineData("epp", null)]
    public async Task Exige_tipo_y_descripcion(string? tipo, string? descripcion)
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarDescritoAsync(cliente);

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/controles", new { tipo, descripcion });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Rechaza_una_sugerencia_de_otro_peligro()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var ajena = await CrearPlantillaAsync(await CrearPeligroAsync(), TipoControl.Epp, "Guantes");
        var riesgo = await AgregarDelCatalogoAsync(cliente, await CrearPeligroAsync());

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgo}/controles", new { tipo = "epp", descripcion = "Guantes", plantillaId = ajena });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("riesgos.sugerencia_invalida", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Un_riesgo_que_no_esta_en_la_matriz_no_recibe_controles()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        Guid sugerido;
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            var encuesta = new Encuesta { Titulo = "Diagnóstico", Estado = EstadoEncuesta.Publicada, CreadaPorId = empresa.AdminVinculoId };
            var riesgo = new RiesgoEmpresa { PeligroId = peligro, EncuestaId = encuesta.Id, Estado = EstadoRiesgo.Sugerido };
            db.AddRange(encuesta, riesgo);
            await db.SaveChangesAsync();
            sugerido = riesgo.Id;
        }

        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{sugerido}/controles", new { tipo = "epp", descripcion = "Casco" });

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("riesgos.no_en_matriz", (await Escenario.Json(r)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Senala_los_riesgos_altos_y_criticos_sin_control()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        await AgregarEvaluadoAsync(cliente, "Alto sin control", 3, 4);
        await AgregarEvaluadoAsync(cliente, "Crítico sin control", 5, 5);
        var conControl = await AgregarEvaluadoAsync(cliente, "Alto con control", 2, 5);
        await Crear(cliente, conControl, "ingenieria", "Barandal");
        await AgregarEvaluadoAsync(cliente, "Medio sin control", 3, 3);
        await AgregarDescritoAsync(cliente);

        var matriz = await Escenario.Json(await cliente.GetAsync("/api/riesgos/matriz"));
        var nombres = (await Escenario.Json(await cliente.GetAsync("/api/riesgos?sinControl=true"))).EnumerateArray()
            .Select(x => x.GetProperty("descripcionPeligro").GetString()).ToArray();

        Assert.Equal(2, matriz.GetProperty("altosSinControl").GetInt32());
        Assert.Equal(new[] { "Crítico sin control", "Alto sin control" }, nombres);
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.GetAsync("/api/riesgos?sinControl=true&nivel=medio")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.GetAsync("/api/riesgos?sinControl=true&sinEvaluar=true")).StatusCode);
    }

    [Fact]
    public async Task Otra_empresa_no_ve_ni_agrega_controles_de_un_riesgo_ajeno()
    {
        var (empresaA, clienteA) = await EmpresaConResponsableAsync();
        var (_, clienteB) = await EmpresaConResponsableAsync();
        var riesgoA = await AgregarDescritoAsync(clienteA);
        await Crear(clienteA, riesgoA, "epp", "Casco");

        Assert.Equal(HttpStatusCode.NotFound, (await clienteB.GetAsync($"/api/riesgos/{riesgoA}/controles")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await clienteB.GetAsync($"/api/riesgos/{riesgoA}/controles/sugeridos")).StatusCode);
        var r = await clienteB.PostAsJsonAsync($"/api/riesgos/{riesgoA}/controles", new { tipo = "epp", descripcion = "Intruso" });
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        await using var db = api.NuevoContexto(empresaA.EmpresaId);
        Assert.Equal(1, await db.Control.CountAsync(c => c.RiesgoEmpresaId == riesgoA));
    }

    [Fact]
    public async Task Un_colaborador_no_define_controles()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var riesgo = await AgregarDescritoAsync(cliente);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync($"/api/riesgos/{riesgo}/controles")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await colaborador.PostAsJsonAsync($"/api/riesgos/{riesgo}/controles", new { tipo = "epp", descripcion = "Casco" })).StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Controles", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    private async Task<Guid> CrearPeligroAsync()
    {
        var peligro = new Peligro { Codigo = $"p{Guid.NewGuid():N}"[..20], Nombre = $"Peligro {Guid.NewGuid():N}"[..20], Clasificacion = ClasificacionPeligro.Mecanico };
        await using var db = api.NuevoContexto(empresaId: null);
        db.Peligro.Add(peligro);
        await db.SaveChangesAsync();
        return peligro.Id;
    }

    private async Task<Guid> CrearPlantillaAsync(Guid peligroId, TipoControl? tipo, string titulo, bool activa = true)
    {
        var plantilla = new PlantillaTarea { PeligroId = peligroId, TipoControl = tipo, Titulo = titulo, PrioridadSugerida = 1, Activa = activa };
        await using var db = api.NuevoContexto(empresaId: null);
        db.PlantillaTarea.Add(plantilla);
        await db.SaveChangesAsync();
        return plantilla.Id;
    }

    private static async Task<Guid> AgregarDelCatalogoAsync(HttpClient cliente, Guid peligroId)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId, actividad = "Taller" });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> AgregarDescritoAsync(HttpClient cliente, string? descripcion = null)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos",
            new { descripcionPeligro = descripcion ?? $"Peligro {Guid.NewGuid():N}"[..20], actividad = "Planta" });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> AgregarEvaluadoAsync(HttpClient cliente, string descripcion, int probabilidad, int severidad)
    {
        var id = await AgregarDescritoAsync(cliente, descripcion);
        (await cliente.PostAsJsonAsync($"/api/riesgos/{id}/evaluaciones", new { probabilidad, severidad })).EnsureSuccessStatusCode();
        return id;
    }

    private static async Task Crear(HttpClient cliente, Guid riesgoId, string tipo, string descripcion) =>
        (await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/controles", new { tipo, descripcion })).EnsureSuccessStatusCode();
}
