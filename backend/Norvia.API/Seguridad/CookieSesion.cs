using Microsoft.Extensions.Options;
using Norvia.Application.Features.Auth.DTOs;
using Norvia.Infrastructure.Opciones;

namespace Norvia.API.Seguridad;

/// <summary>
/// Emite y lee cookies de sesión (refresh HttpOnly + access como respaldo del Bearer).
/// Flags desde <see cref="CookiesOpciones"/> (.env Cookies__*).
/// </summary>
public sealed class CookieSesion(IOptions<CookiesOpciones> opciones, IOptions<JwtOpciones> jwt)
{
    private readonly CookiesOpciones _cookies = opciones.Value;
    private readonly JwtOpciones _jwt = jwt.Value;

    public void Establecer(HttpResponse response, SesionResponseDto sesion)
    {
        response.Cookies.Append(_cookies.RefreshName, sesion.RefreshToken, CrearOpciones(
            TimeSpan.FromDays(_jwt.RefreshDias)));
        response.Cookies.Append(_cookies.AccessName, sesion.AccessToken, CrearOpciones(
            TimeSpan.FromMinutes(_jwt.AccessMinutos)));
    }

    public void Borrar(HttpResponse response)
    {
        var borrar = CrearOpciones(TimeSpan.Zero);
        borrar.Expires = DateTimeOffset.UnixEpoch;
        response.Cookies.Append(_cookies.RefreshName, string.Empty, borrar);
        response.Cookies.Append(_cookies.AccessName, string.Empty, borrar);
    }

    public string? LeerRefresh(HttpRequest request) =>
        request.Cookies.TryGetValue(_cookies.RefreshName, out var valor) && !string.IsNullOrWhiteSpace(valor)
            ? valor
            : null;

    public string? LeerAccess(HttpRequest request) =>
        request.Cookies.TryGetValue(_cookies.AccessName, out var valor) && !string.IsNullOrWhiteSpace(valor)
            ? valor
            : null;

    private CookieOptions CrearOpciones(TimeSpan vida) => new()
    {
        HttpOnly = _cookies.HttpOnly,
        Secure = _cookies.Secure,
        SameSite = ParseSameSite(_cookies.SameSite),
        Domain = string.IsNullOrWhiteSpace(_cookies.Domain) ? null : _cookies.Domain,
        Path = string.IsNullOrWhiteSpace(_cookies.Path) ? "/" : _cookies.Path,
        MaxAge = vida,
        IsEssential = true,
    };

    private static SameSiteMode ParseSameSite(string valor) =>
        Enum.TryParse<SameSiteMode>(valor, ignoreCase: true, out var modo)
            ? modo
            : SameSiteMode.Lax;
}
