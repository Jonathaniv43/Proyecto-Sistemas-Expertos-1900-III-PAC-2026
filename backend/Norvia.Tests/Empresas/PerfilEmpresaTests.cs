using System.Net;
using System.Net.Http.Json;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Empresas;

/// <summary>HU-026: GET y PATCH del perfil de la empresa de la sesión.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class PerfilEmpresaTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    [Fact]
    public async Task Admin_puede_obtener_el_perfil_de_su_empresa()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PerfilGet");

        var r = await empresa.Cliente.GetAsync("/api/empresas/perfil");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal(empresa.EmpresaId, json.GetProperty("empresaId").GetGuid());
        Assert.False(json.GetProperty("perfilCompleto").GetBoolean());
        Assert.Equal("activa", json.GetProperty("estado").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("rtn").GetString()));
        Assert.Contains("S. de R.L.", json.GetProperty("razonSocial").GetString());
    }

    [Fact]
    public async Task Admin_actualiza_trabajadores_y_fecha_sin_cambiar_razon_social_ni_rtn()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PerfilPatchParcial");
        var antes = await Escenario.Json(await empresa.Cliente.GetAsync("/api/empresas/perfil"));
        var razon = antes.GetProperty("razonSocial").GetString();
        var rtn = antes.GetProperty("rtn").GetString();

        var r = await empresa.Cliente.PatchAsJsonAsync("/api/empresas/perfil", new
        {
            trabajadoresPermanentes = 45,
            fechaInicioOperaciones = "2015-03-01"
        });

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal(45, json.GetProperty("trabajadoresPermanentes").GetInt32());
        Assert.Equal("2015-03-01", json.GetProperty("fechaInicioOperaciones").GetString());
        Assert.Equal(razon, json.GetProperty("razonSocial").GetString());
        Assert.Equal(rtn, json.GetProperty("rtn").GetString());
        Assert.False(json.GetProperty("perfilCompleto").GetBoolean()); // falta rubro
    }

    [Fact]
    public async Task Admin_completa_perfil_con_rubro_y_trabajadores()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PerfilCompleto");

        var r = await empresa.Cliente.PatchAsJsonAsync("/api/empresas/perfil", new
        {
            rubroCodigo = "comercio",
            trabajadoresPermanentes = 12,
            direccion = "Col. Palmira",
            actividadEconomica = "Comercio al por mayor"
        });

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.True(json.GetProperty("perfilCompleto").GetBoolean());
        Assert.Equal(2, json.GetProperty("rubroId").GetInt16());
        Assert.Equal("Col. Palmira", json.GetProperty("direccion").GetString());
        Assert.Equal("Comercio al por mayor", json.GetProperty("actividadEconomica").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("rubroNombre").GetString()));
    }

    [Fact]
    public async Task Patch_con_rubro_invalido_responde_400()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PerfilRubroMal");

        var r = await empresa.Cliente.PatchAsJsonAsync("/api/empresas/perfil", new { rubroCodigo = "no_existe" });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Patch_vacio_responde_400()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PerfilVacio");

        var r = await empresa.Cliente.PatchAsJsonAsync("/api/empresas/perfil", new { });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Responsable_sst_puede_leer_pero_no_actualizar()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PerfilRoles");
        var sst = await _escenario.ClienteResponsableSstAsync(
            empresa, $"sst.{Guid.NewGuid():N}@pruebas.hn");

        var get = await sst.GetAsync("/api/empresas/perfil");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var patch = await sst.PatchAsJsonAsync("/api/empresas/perfil", new { trabajadoresPermanentes = 3 });
        Assert.Equal(HttpStatusCode.Forbidden, patch.StatusCode);
    }

    [Fact]
    public async Task Colaborador_no_puede_leer_el_perfil()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PerfilColab");
        var email = $"colab.{Guid.NewGuid():N}@pruebas.hn";
        await Escenario.AgregarColaboradorAsync(empresa, email);
        var colab = await _escenario.ClienteEmpresaAsync(email, "Temporal#2026", empresa.IdEmpresaCifrado);

        var get = await colab.GetAsync("/api/empresas/perfil");
        Assert.Equal(HttpStatusCode.Forbidden, get.StatusCode);
    }
}
