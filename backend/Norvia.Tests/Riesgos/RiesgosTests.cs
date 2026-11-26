using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Riesgos;

/// <summary>HU-036: agregar riesgos a la matriz de la empresa y consultarlos.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class RiesgosTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Agrega_un_riesgo_con_peligro_del_catalogo_y_queda_confirmado_en_la_matriz()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();

        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId = peligro.Id, actividad = "  Bodega de carga  " });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal("confirmado", json.GetProperty("estado").GetString());
        Assert.Equal(peligro.Nombre, json.GetProperty("peligroNombre").GetString());
        Assert.Equal("Bodega de carga", json.GetProperty("actividad").GetString());
        Assert.Equal(JsonValueKind.Null, json.GetProperty("encuestaId").ValueKind);

        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var riesgo = await db.RiesgoEmpresa.AsNoTracking().SingleAsync(x => x.Id == json.GetProperty("id").GetGuid());
        Assert.Equal(empresa.EmpresaId, riesgo.EmpresaId);
        Assert.NotNull(riesgo.RevisadoPorId);
    }

    [Fact]
    public async Task Agrega_un_riesgo_con_un_peligro_descrito()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();

        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = "Piso resbaloso junto a la máquina de café", actividad = "Cocina" });

        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal(JsonValueKind.Null, json.GetProperty("peligroId").ValueKind);
        Assert.Equal("Piso resbaloso junto a la máquina de café", json.GetProperty("descripcionPeligro").GetString());
    }

    [Theory]
    [InlineData(false, false, "Cocina")]   // ni peligro ni descripción
    [InlineData(true, true, "Cocina")]     // los dos
    [InlineData(true, false, "")]          // sin actividad
    public async Task Rechaza_solicitudes_invalidas(bool conPeligro, bool conDescripcion, string actividad)
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();

        var r = await cliente.PostAsJsonAsync("/api/riesgos", new
        {
            peligroId = conPeligro ? peligro.Id : (Guid?)null,
            descripcionPeligro = conDescripcion ? "Algo" : null,
            actividad
        });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Rechaza_un_peligro_inexistente_o_inactivo()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var inactivo = await CrearPeligroAsync(activo: false);

        foreach (var id in new[] { Guid.NewGuid(), inactivo.Id })
        {
            var r = await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId = id, actividad = "Bodega" });
            Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
            Assert.Equal("riesgos.peligro_invalido", (await Escenario.Json(r)).GetProperty("codigo").GetString());
        }
    }

    [Fact]
    public async Task No_repite_el_mismo_peligro_con_la_misma_actividad()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId = peligro.Id, actividad = "Bodega" });

        var repetido = await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId = peligro.Id, actividad = "BODEGA" });
        var otraActividad = await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId = peligro.Id, actividad = "Patio" });

        Assert.Equal(HttpStatusCode.Conflict, repetido.StatusCode);
        Assert.Equal("riesgos.duplicado", (await Escenario.Json(repetido)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.Created, otraActividad.StatusCode);
    }

    [Fact]
    public async Task La_matriz_lista_los_confirmados_con_su_origen_y_no_los_sugeridos_ni_descartados()
    {
        var (empresa, cliente) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        var manual = await Escenario.Json(await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId = peligro.Id, actividad = "Bodega" }));
        var (encuestaId, confirmado, sugerido, descartado) = await CrearRiesgosDeEncuestaAsync(empresa);

        var lista = (await Escenario.Json(await cliente.GetAsync("/api/riesgos"))).EnumerateArray().ToList();
        var ids = lista.Select(x => x.GetProperty("id").GetGuid()).ToList();

        Assert.Contains(manual.GetProperty("id").GetGuid(), ids);
        Assert.Contains(confirmado, ids);
        Assert.DoesNotContain(sugerido, ids);
        Assert.DoesNotContain(descartado, ids);
        var deEncuesta = lista.Single(x => x.GetProperty("id").GetGuid() == confirmado);
        Assert.Equal(encuestaId, deEncuesta.GetProperty("encuestaId").GetGuid());
        Assert.Equal("Diagnóstico 2026", deEncuesta.GetProperty("encuestaTitulo").GetString());
    }

    [Fact]
    public async Task Otra_empresa_no_ve_ni_consulta_los_riesgos_ajenos()
    {
        var (_, clienteA) = await EmpresaConResponsableAsync();
        var (_, clienteB) = await EmpresaConResponsableAsync();
        var peligro = await CrearPeligroAsync();
        var riesgoA = (await Escenario.Json(await clienteA.PostAsJsonAsync("/api/riesgos", new { peligroId = peligro.Id, actividad = "Bodega" })))
            .GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.NotFound, (await clienteB.GetAsync($"/api/riesgos/{riesgoA}")).StatusCode);
        Assert.DoesNotContain(riesgoA, (await Escenario.Json(await clienteB.GetAsync("/api/riesgos"))).EnumerateArray()
            .Select(x => x.GetProperty("id").GetGuid()));
        Assert.Equal(HttpStatusCode.OK, (await clienteA.GetAsync($"/api/riesgos/{riesgoA}")).StatusCode);
        // B puede agregar el mismo peligro con la misma actividad: la matriz es de cada empresa.
        Assert.Equal(HttpStatusCode.Created,
            (await clienteB.PostAsJsonAsync("/api/riesgos", new { peligroId = peligro.Id, actividad = "Bodega" })).StatusCode);
    }

    [Fact]
    public async Task El_catalogo_para_elegir_solo_trae_peligros_activos()
    {
        var (_, cliente) = await EmpresaConResponsableAsync();
        var activo = await CrearPeligroAsync();
        var inactivo = await CrearPeligroAsync(activo: false);

        var ids = (await Escenario.Json(await cliente.GetAsync("/api/riesgos/peligros-disponibles"))).EnumerateArray()
            .Select(x => x.GetProperty("id").GetGuid()).ToList();

        Assert.Contains(activo.Id, ids);
        Assert.DoesNotContain(inactivo.Id, ids);
    }

    [Fact]
    public async Task Un_colaborador_no_tiene_acceso()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Riesgos", 1);
        var colaborador = await _escenario.ClienteConRolAsync(empresa, "colaborador");

        Assert.Equal(HttpStatusCode.Forbidden, (await colaborador.GetAsync("/api/riesgos")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await colaborador.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro = "Algo", actividad = "Bodega" })).StatusCode);
    }

    // ---------- Apoyo ----------

    private async Task<(EmpresaDePrueba Empresa, HttpClient Responsable)> EmpresaConResponsableAsync()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Riesgos", 1);
        return (empresa, await _escenario.ClienteConRolAsync(empresa, "responsable_sst"));
    }

    private async Task<Peligro> CrearPeligroAsync(bool activo = true)
    {
        var peligro = new Peligro
        {
            Codigo = $"p{Guid.NewGuid():N}"[..20], Nombre = $"Peligro {Guid.NewGuid():N}"[..20],
            Clasificacion = ClasificacionPeligro.Locativo, Activo = activo
        };
        await using var db = api.NuevoContexto(empresaId: null);
        db.Peligro.Add(peligro);
        await db.SaveChangesAsync();
        return peligro;
    }

    /// <summary>Una encuesta con tres sugerencias: una confirmada (HU-035), una sin revisar y una descartada.</summary>
    private async Task<(Guid EncuestaId, Guid Confirmado, Guid Sugerido, Guid Descartado)> CrearRiesgosDeEncuestaAsync(EmpresaDePrueba empresa)
    {
        var peligros = new[] { await CrearPeligroAsync(), await CrearPeligroAsync(), await CrearPeligroAsync() };
        await using var db = api.NuevoContexto(empresa.EmpresaId);
        var encuesta = new Encuesta { Titulo = "Diagnóstico 2026", Estado = EstadoEncuesta.Publicada, CreadaPorId = empresa.AdminVinculoId };
        db.Encuesta.Add(encuesta);
        var riesgos = new[] { EstadoRiesgo.Confirmado, EstadoRiesgo.Sugerido, EstadoRiesgo.Descartado }
            .Select((estado, i) => new RiesgoEmpresa { PeligroId = peligros[i].Id, EncuestaId = encuesta.Id, Estado = estado })
            .ToArray();
        db.RiesgoEmpresa.AddRange(riesgos);
        await db.SaveChangesAsync();
        return (encuesta.Id, riesgos[0].Id, riesgos[1].Id, riesgos[2].Id);
    }
}
