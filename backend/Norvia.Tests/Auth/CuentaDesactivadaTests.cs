using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Auth;

/// <summary>HU-018: una cuenta desactivada (Usuario.Activo = false) no inicia sesión ni renueva la que tenía.</summary>
[Collection(ColeccionApi.Nombre)]
public sealed class CuentaDesactivadaTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);

    private async Task DesactivarUsuarioAsync(Guid empresaId)
    {
        await using var db = api.NuevoContexto(empresaId: null);
        var personaId = await db.VinculoEmpresa.IgnoreQueryFilters()
            .Where(v => v.EmpresaId == empresaId).Select(v => v.PersonaId).SingleAsync();
        await db.Usuario.Where(u => u.PersonaId == personaId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Activo, false));
    }

    [Fact]
    public async Task Cuenta_desactivada_no_inicia_sesion_y_recibe_el_mismo_mensaje_generico()
    {
        var empresa = await _escenario.CrearEmpresaAsync("CuentaInactiva");
        await DesactivarUsuarioAsync(empresa.EmpresaId);

        var r = await _escenario.LoginEmpresaAsync(empresa.AdminEmail, Escenario.PasswordAdmin, empresa.IdEmpresaCifrado);

        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        var json = await Escenario.Json(r);
        Assert.Equal("auth.credenciales_invalidas", json.GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Cuenta_desactivada_pierde_la_sesion_al_renovar()
    {
        var empresa = await _escenario.CrearEmpresaAsync("CuentaInactivaRefresh");
        var login = await _escenario.LoginEmpresaAsync(empresa.AdminEmail, Escenario.PasswordAdmin, empresa.IdEmpresaCifrado);
        var refresh = (await Escenario.Json(login)).GetProperty("refreshToken").GetString();
        await DesactivarUsuarioAsync(empresa.EmpresaId);

        var r = await api.CreateClient().PostAsJsonAsync("/api/auth/refresh", new { refreshToken = refresh });

        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }
}
