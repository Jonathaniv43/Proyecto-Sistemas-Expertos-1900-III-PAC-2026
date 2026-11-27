using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Application.Common.Interfaces;
using Norvia.Tests.Infraestructura;

namespace Norvia.Tests.Auth;

/// <summary>HU-022: cambio de contraseña confirmado desde el correo (enlace firmado, de un solo uso).</summary>
[Collection(ColeccionApi.Nombre)]
public sealed partial class CambioContrasenaTests(NorviaApiFactory api)
{
    private readonly Escenario _escenario = new(api);
    private const string Nueva = "Cambiada#Segura2026";

    [Fact]
    public async Task La_solicitud_responde_igual_exista_o_no_la_cuenta_y_solo_escribe_a_la_que_existe()
    {
        var empresa = await _escenario.CrearEmpresaAsync("SolicitudCambio");
        var inexistente = $"nadie.{Guid.NewGuid():N}@pruebas.hn";

        var r1 = await Solicitar(empresa.AdminEmail);
        var r2 = await Solicitar(inexistente);

        Assert.Equal(HttpStatusCode.Accepted, r1.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, r2.StatusCode);
        Assert.Equal(await r1.Content.ReadAsStringAsync(), await r2.Content.ReadAsStringAsync());
        Assert.Single(api.Correos.Para(empresa.AdminEmail));
        Assert.Empty(api.Correos.Para(inexistente));
        Assert.Contains("/restablecer-contrasena?token=", api.Correos.Para(empresa.AdminEmail)[0].CuerpoHtml);
    }

    [Fact]
    public async Task Con_el_enlace_cambia_la_contrasena_en_todas_las_empresas_y_cierra_todas_las_sesiones()
    {
        var a = await _escenario.CrearEmpresaAsync("CambioA");
        var b = await _escenario.CrearEmpresaAsync("CambioB");
        var email = $"varias.{Guid.NewGuid():N}@pruebas.hn";
        await Escenario.AgregarColaboradorAsync(a, email);
        await Escenario.AgregarColaboradorAsync(b, email); // misma cuenta (RN-65)

        var sesionA = await Escenario.Json(await _escenario.LoginEmpresaAsync(email, "Temporal#2026", a.IdEmpresaCifrado));
        var sesionB = await Escenario.Json(await _escenario.LoginEmpresaAsync(email, "Temporal#2026", b.IdEmpresaCifrado));

        await Solicitar(email);
        var confirmar = await Confirmar(TokenDe(email), Nueva);
        Assert.Equal(HttpStatusCode.NoContent, confirmar.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await _escenario.LoginEmpresaAsync(email, Nueva, a.IdEmpresaCifrado)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _escenario.LoginEmpresaAsync(email, Nueva, b.IdEmpresaCifrado)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _escenario.LoginEmpresaAsync(email, "Temporal#2026", a.IdEmpresaCifrado)).StatusCode);
        foreach (var sesion in new[] { sesionA, sesionB })
        {
            var refresh = await api.CreateClient().PostAsJsonAsync("/api/auth/refresh",
                new { refreshToken = sesion.GetProperty("refreshToken").GetString() });
            Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
        }

        await using var db = api.NuevoContexto(empresaId: null);
        Assert.NotNull((await db.Acceso.SingleAsync(x => x.Email == email)).PasswordHash);
    }

    [Fact]
    public async Task El_enlace_sirve_una_sola_vez()
    {
        var empresa = await _escenario.CrearEmpresaAsync("UnSoloUso");
        await Solicitar(empresa.AdminEmail);
        var token = TokenDe(empresa.AdminEmail);

        Assert.Equal(HttpStatusCode.NoContent, (await Confirmar(token, Nueva)).StatusCode);
        var otraVez = await Confirmar(token, "Otra#Distinta2026");

        Assert.Equal(HttpStatusCode.Gone, otraVez.StatusCode);
        Assert.Equal("auth.enlace_invalido", (await Escenario.Json(otraVez)).GetProperty("codigo").GetString());
        Assert.Equal(HttpStatusCode.OK, (await _escenario.LoginEmpresaAsync(empresa.AdminEmail, Nueva, empresa.IdEmpresaCifrado)).StatusCode);
    }

    [Fact]
    public async Task Un_enlace_vencido_o_alterado_no_sirve()
    {
        var empresa = await _escenario.CrearEmpresaAsync("Vencido");
        await using var db = api.NuevoContexto(empresaId: null);
        var acceso = await db.Acceso.SingleAsync(x => x.Email == empresa.AdminEmail);
        var tokens = api.Services.GetRequiredService<ITokenCambioContrasena>();
        var vencido = tokens.Crear(acceso.Id, acceso.PasswordHash, DateTime.UtcNow.AddMinutes(-1));

        Assert.Equal(HttpStatusCode.Gone, (await Confirmar(vencido, Nueva)).StatusCode);

        var vigente = tokens.Crear(acceso.Id, acceso.PasswordHash, DateTime.UtcNow.AddMinutes(30));
        var alterado = (vigente[0] == 'A' ? 'B' : 'A') + vigente[1..];
        Assert.Equal(HttpStatusCode.Gone, (await Confirmar(alterado, Nueva)).StatusCode);
        Assert.Equal(HttpStatusCode.Gone, (await Confirmar("no-es-un-token", Nueva)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await _escenario.LoginEmpresaAsync(empresa.AdminEmail, Escenario.PasswordAdmin, empresa.IdEmpresaCifrado)).StatusCode);
    }

    [Fact]
    public async Task La_contrasena_nueva_cumple_la_politica_y_no_repite_la_actual()
    {
        var empresa = await _escenario.CrearEmpresaAsync("PoliticaCambio");
        await Solicitar(empresa.AdminEmail);
        var token = TokenDe(empresa.AdminEmail);

        var debil = await Confirmar(token, "corta");
        Assert.Equal(HttpStatusCode.BadRequest, debil.StatusCode);
        Assert.True((await Escenario.Json(debil)).GetProperty("errors").TryGetProperty("ContrasenaNueva", out _));

        var igual = await Confirmar(token, Escenario.PasswordAdmin);
        Assert.Equal(HttpStatusCode.BadRequest, igual.StatusCode);
        Assert.Equal("auth.contrasena_igual_actual", (await Escenario.Json(igual)).GetProperty("codigo").GetString());

        // Los intentos rechazados no gastan el enlace.
        Assert.Equal(HttpStatusCode.NoContent, (await Confirmar(token, Nueva)).StatusCode);
    }

    [Fact]
    public async Task El_token_de_sesion_lleva_el_correo_de_la_cuenta()
    {
        var empresa = await _escenario.CrearEmpresaAsync("ClaimEmail");
        var login = await Escenario.Json(await _escenario.LoginEmpresaAsync(empresa.AdminEmail, Escenario.PasswordAdmin, empresa.IdEmpresaCifrado));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(login.GetProperty("accessToken").GetString());
        Assert.Equal(empresa.AdminEmail, jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    private Task<HttpResponseMessage> Solicitar(string email) =>
        api.CreateClient().PostAsJsonAsync("/api/auth/contrasena/solicitud", new { email });

    private Task<HttpResponseMessage> Confirmar(string token, string contrasenaNueva) =>
        api.CreateClient().PostAsJsonAsync("/api/auth/contrasena/confirmacion", new { token, contrasenaNueva });

    private string TokenDe(string email) =>
        Uri.UnescapeDataString(EnlaceRegex().Match(api.Correos.Para(email)[^1].CuerpoHtml).Groups[1].Value);

    [GeneratedRegex("restablecer-contrasena\\?token=([^\"&<]+)")]
    private static partial Regex EnlaceRegex();
}
