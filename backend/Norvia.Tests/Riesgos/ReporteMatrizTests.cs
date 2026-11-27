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

/// <summary>HU-075: reporte oficial de la matriz de riesgos para imprimir o guardar en PDF al vuelo (art. 47.b, RN-52).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class ReporteMatrizTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Incluye_empresa_fecha_mapa_riesgos_confirmados_evaluaciones_y_controles_ordenados()
    {
        var empresa = await _escenario.CrearEmpresaAsync("RepMat");
        Assert.Equal(HttpStatusCode.OK, (await empresa.Cliente.PatchAsJsonAsync("/api/empresas/perfil", new
        {
            rubroCodigo = "comercio",
            trabajadoresPermanentes = 30,
            direccion = "Blvd. Suyapa",
            actividadEconomica = "Venta al detalle",
            fechaInicioOperaciones = "2020-01-15"
        })).StatusCode);

        var sst = await _escenario.ClienteConRolAsync(empresa, "responsable_sst");
        var peligroCatId = await CrearPeligroAsync("Cableado expuesto", ClasificacionPeligro.Electrico);
        var peligroSugId = await CrearPeligroAsync("Peligro sugerido oculto", ClasificacionPeligro.Fisico);
        var peligroDescId = await CrearPeligroAsync("Peligro descartado oculto", ClasificacionPeligro.Quimico);

        var critico = await CrearRiesgoCatalogoAsync(sst, peligroCatId, "Tablero eléctrico");
        Assert.Equal(HttpStatusCode.Created,
            (await sst.PostAsJsonAsync($"/api/riesgos/{critico}/evaluaciones", new { probabilidad = 4, severidad = 5 })).StatusCode);

        // Agregar controles fuera del orden jerárquico RN-21 para verificar ordenación en el reporte.
        await CrearControlAsync(sst, critico, "epp", "Guantes dieléctricos");
        await CrearControlAsync(sst, critico, "administrativo", "Señalización de alto voltaje");
        await CrearControlAsync(sst, critico, "eliminacion", "Reemplazar tablero dañado");

        Assert.Equal(HttpStatusCode.Created,
            (await sst.PostAsJsonAsync($"/api/riesgos/{critico}/evaluaciones-residual", new { probabilidad = 2, severidad = 2 })).StatusCode);

        var altoSinControl = await CrearRiesgoDescritoAsync(sst, "Carga manual pesada", "Bodega");
        Assert.Equal(HttpStatusCode.Created,
            (await sst.PostAsJsonAsync($"/api/riesgos/{altoSinControl}/evaluaciones", new { probabilidad = 3, severidad = 4 })).StatusCode);

        var sinEvaluar = await CrearRiesgoDescritoAsync(sst, "Iluminación deficiente", "Pasillo");

        // Riesgos no confirmados (sugerido / descartado) no deben entrar al reporte.
        await using (var db = api.NuevoContexto(empresa.EmpresaId))
        {
            var encuesta = new Encuesta { Titulo = "Diagnóstico", Estado = EstadoEncuesta.Publicada, CreadaPorId = empresa.AdminVinculoId };
            db.Encuesta.Add(encuesta);
            db.RiesgoEmpresa.AddRange(
                new RiesgoEmpresa { PeligroId = peligroSugId, EncuestaId = encuesta.Id, Estado = EstadoRiesgo.Sugerido },
                new RiesgoEmpresa { PeligroId = peligroDescId, EncuestaId = encuesta.Id, Estado = EstadoRiesgo.Descartado });
            await db.SaveChangesAsync();
        }

        var antes = DateTime.UtcNow.AddSeconds(-2);
        var resp = await sst.GetAsync("/api/riesgos/matriz/reporte");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var json = await Escenario.Json(resp);
        var generadoEn = json.GetProperty("generadoEn").GetDateTime();
        Assert.True(generadoEn >= antes && generadoEn <= DateTime.UtcNow.AddSeconds(5));

        var emp = json.GetProperty("empresa");
        Assert.Contains("RepMat", emp.GetProperty("razonSocial").GetString());
        Assert.False(string.IsNullOrWhiteSpace(emp.GetProperty("rtn").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(emp.GetProperty("rubroNombre").GetString()));
        Assert.Equal("Venta al detalle", emp.GetProperty("actividadEconomica").GetString());
        Assert.Equal("Blvd. Suyapa", emp.GetProperty("direccion").GetString());
        Assert.Equal(30, emp.GetProperty("trabajadoresPermanentes").GetInt32());
        Assert.Equal("2020-01-15", emp.GetProperty("fechaInicioOperaciones").GetString());

        var matriz = json.GetProperty("matriz");
        Assert.Equal(3, matriz.GetProperty("total").GetInt32());
        Assert.Equal(1, matriz.GetProperty("sinEvaluar").GetInt32());
        Assert.Equal(1, matriz.GetProperty("altosSinControl").GetInt32());

        var riesgos = json.GetProperty("riesgos").EnumerateArray().ToList();
        Assert.Equal(new[] { critico, altoSinControl, sinEvaluar }, riesgos.Select(r => r.GetProperty("id").GetGuid()));
        Assert.DoesNotContain(riesgos, r => r.GetProperty("peligroNombre").GetString()?.Contains("oculto") == true);

        var primero = riesgos[0];
        Assert.Equal("Cableado expuesto", primero.GetProperty("peligroNombre").GetString());
        Assert.Equal("electrico", primero.GetProperty("peligroClasificacion").GetString());
        Assert.Equal("Tablero eléctrico", primero.GetProperty("actividad").GetString());
        Assert.Equal(20, primero.GetProperty("evaluacion").GetProperty("nivel").GetInt32());
        Assert.Equal("critico", primero.GetProperty("evaluacion").GetProperty("clasificacion").GetString());
        Assert.Equal(4, primero.GetProperty("evaluacionResidual").GetProperty("nivel").GetInt32());
        Assert.Equal("bajo", primero.GetProperty("evaluacionResidual").GetProperty("clasificacion").GetString());

        var controles = primero.GetProperty("controles").EnumerateArray().ToList();
        Assert.Equal(new[] { "eliminacion", "administrativo", "epp" }, controles.Select(c => c.GetProperty("tipo").GetString()));
        Assert.Equal("Reemplazar tablero dañado", controles[0].GetProperty("descripcion").GetString());

        Assert.Equal(JsonValueKind.Null, riesgos[2].GetProperty("evaluacion").ValueKind);
        Assert.Empty(riesgos[2].GetProperty("controles").EnumerateArray());
    }

    [Fact]
    public async Task Con_perfil_incompleto_y_sin_riesgos_confirmados_responde_200_vacio()
    {
        var empresa = await _escenario.CrearEmpresaAsync("RepVacio");

        var resp = await empresa.Cliente.GetAsync("/api/riesgos/matriz/reporte");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var json = await Escenario.Json(resp);
        var emp = json.GetProperty("empresa");
        Assert.Equal(JsonValueKind.Null, emp.GetProperty("rubroNombre").ValueKind);
        Assert.Equal(JsonValueKind.Null, emp.GetProperty("trabajadoresPermanentes").ValueKind);
        Assert.Equal(0, json.GetProperty("matriz").GetProperty("total").GetInt32());
        Assert.Empty(json.GetProperty("riesgos").EnumerateArray());
    }

    [Fact]
    public async Task Solo_roles_de_gestion_consultan_y_con_aislamiento_entre_empresas()
    {
        var a = await _escenario.CrearEmpresaAsync("RepIsoA");
        var b = await _escenario.CrearEmpresaAsync("RepIsoB");
        var sstA = await _escenario.ClienteConRolAsync(a, "responsable_sst");
        var colabA = await _escenario.ClienteConRolAsync(a, "colaborador");
        var encuestadoA = await _escenario.ClienteConRolAsync(a, "encuestado");

        var riesgoA = await CrearRiesgoDescritoAsync(sstA, "Peligro exclusivo de A", "Zona A");

        Assert.Equal(HttpStatusCode.OK, (await a.Cliente.GetAsync("/api/riesgos/matriz/reporte")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await sstA.GetAsync("/api/riesgos/matriz/reporte")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await colabA.GetAsync("/api/riesgos/matriz/reporte")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await encuestadoA.GetAsync("/api/riesgos/matriz/reporte")).StatusCode);

        var repB = await Escenario.Json(await b.Cliente.GetAsync("/api/riesgos/matriz/reporte"));
        Assert.Contains("RepIsoB", repB.GetProperty("empresa").GetProperty("razonSocial").GetString());
        Assert.Equal(0, repB.GetProperty("matriz").GetProperty("total").GetInt32());
        var idsB = repB.GetProperty("riesgos").EnumerateArray().Select(r => r.GetProperty("id").GetGuid()).ToList();
        Assert.DoesNotContain(riesgoA, idsB);
    }

    // ---------- Apoyo ----------

    private async Task<Guid> CrearPeligroAsync(string nombre, ClasificacionPeligro clasificacion)
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var p = new Peligro { Codigo = $"pm_{Guid.NewGuid():N}"[..20], Nombre = nombre, Clasificacion = clasificacion };
        db.Peligro.Add(p);
        await db.SaveChangesAsync();
        return p.Id;
    }

    private static async Task<Guid> CrearRiesgoCatalogoAsync(HttpClient cliente, Guid peligroId, string actividad)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { peligroId, actividad });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CrearRiesgoDescritoAsync(HttpClient cliente, string descripcionPeligro, string actividad)
    {
        var r = await cliente.PostAsJsonAsync("/api/riesgos", new { descripcionPeligro, actividad });
        r.EnsureSuccessStatusCode();
        return (await Escenario.Json(r)).GetProperty("id").GetGuid();
    }

    private static async Task CrearControlAsync(HttpClient cliente, Guid riesgoId, string tipo, string descripcion)
    {
        var r = await cliente.PostAsJsonAsync($"/api/riesgos/{riesgoId}/controles", new { tipo, descripcion });
        r.EnsureSuccessStatusCode();
    }
}
