using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Seguridad;

/// <summary>
/// Microsoft Entra ID (OAuth 2.0 código de autorización + PKCE). El <c>id_token</c> llega directo del
/// endpoint de token de Microsoft por TLS, así que se validan audiencia, vencimiento y emisor en lugar
/// de la firma (OpenID Connect Core, 3.1.3.7).
/// </summary>
public sealed class MicrosoftProveedorIdentidad(HttpClient http, IOptions<MicrosoftOpciones> opciones) : IProveedorIdentidad
{
    private static readonly Error IdentidadInvalida =
        new("auth.identidad_invalida", "No se pudo validar la cuenta de Microsoft. Intenta de nuevo.", TipoError.NoAutorizado);

    private static readonly Error NoConfigurado =
        new("auth.microsoft_no_configurado", "El acceso con Microsoft no está configurado.", TipoError.Servidor);

    public ProveedorIdentidad Proveedor => ProveedorIdentidad.Microsoft;

    private sealed record Respuesta([property: JsonPropertyName("id_token")] string? IdToken);

    public async Task<Resultado<IdentidadExterna>> CanjearCodigoAsync(
        string codigo, string codeVerifier, string redirectUri, CancellationToken cancellationToken)
    {
        var o = opciones.Value;
        if (!o.EstaConfigurado) return NoConfigurado;
        if (!o.RedirectPermitido(redirectUri)) return IdentidadInvalida;

        var tenant = string.IsNullOrWhiteSpace(o.Tenant) ? "common" : o.Tenant;
        using var peticion = new HttpRequestMessage(HttpMethod.Post, $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = o.ClientId,
                ["client_secret"] = o.ClientSecret,
                ["grant_type"] = "authorization_code",
                ["code"] = codigo,
                ["redirect_uri"] = redirectUri,
                ["code_verifier"] = codeVerifier,
                ["scope"] = "openid profile email"
            })
        };

        HttpResponseMessage respuesta;
        try
        {
            respuesta = await http.SendAsync(peticion, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return IdentidadInvalida;
        }

        using (respuesta)
        {
            if (!respuesta.IsSuccessStatusCode) return IdentidadInvalida;
            var cuerpo = await respuesta.Content.ReadFromJsonAsync<Respuesta>(cancellationToken);
            if (string.IsNullOrWhiteSpace(cuerpo?.IdToken)) return IdentidadInvalida;
            return LeerIdToken(cuerpo.IdToken, o);
        }
    }

    private static Resultado<IdentidadExterna> LeerIdToken(string idToken, MicrosoftOpciones o)
    {
        JwtSecurityToken jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
        }
        catch (ArgumentException)
        {
            return IdentidadInvalida;
        }

        if (!jwt.Audiences.Contains(o.ClientId)
            || jwt.ValidTo <= DateTime.UtcNow
            || !jwt.Issuer.StartsWith("https://login.microsoftonline.com/", StringComparison.Ordinal))
            return IdentidadInvalida;

        var sub = jwt.Subject;
        // "email" es opcional en cuentas de trabajo; preferred_username es el UPN, que casi siempre es el correo.
        var email = (Claim(jwt, "email") ?? Claim(jwt, "preferred_username"))?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.Length > 254)
            return IdentidadInvalida;

        var nombre = Claim(jwt, "given_name");
        var apellido = Claim(jwt, "family_name");
        if (string.IsNullOrWhiteSpace(nombre))
        {
            // Sin given_name/family_name se divide "name": primera palabra y resto. La persona lo corrige en el formulario.
            var partes = (Claim(jwt, "name") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            nombre = partes.FirstOrDefault() ?? string.Empty;
            apellido = partes.Length > 1 ? string.Join(' ', partes.Skip(1)) : string.Empty;
        }

        return new IdentidadExterna(
            ProveedorIdentidad.Microsoft, sub, email, Recortar(nombre, 80), null, Recortar(apellido ?? string.Empty, 80), null);
    }

    private static string? Claim(JwtSecurityToken jwt, string tipo) => jwt.Claims.FirstOrDefault(c => c.Type == tipo)?.Value;

    private static string Recortar(string texto, int max) => texto.Length <= max ? texto : texto[..max];
}
