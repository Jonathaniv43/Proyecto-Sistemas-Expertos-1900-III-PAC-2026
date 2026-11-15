// norvia.api/Middleware/AutorizacionPermisosMiddleware.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Seguridad;
using Norvia.Domain.Enums;

namespace Norvia.API.Middleware;

public sealed class AutorizacionPermisosMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http, ITenantActual tenant, IAutorizadorPermisos autorizador)
    {
        // Sin endpoint (404 del framework) o endpoint anónimo: no hay nada que verificar.
        if (http.GetEndpoint() is not RouteEndpoint endpoint
            || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(http);
            return;
        }

        if (http.User.Identity?.IsAuthenticated != true)
        {
            await Rechazar(http, StatusCodes.Status401Unauthorized,
                "auth.no_autenticado", "Debe iniciar sesión.");
            return;
        }

        var ct = http.RequestAborted;
        var metodo = http.Request.Method.ToUpperInvariant();
        var ruta = RutaNormalizada.De(endpoint.RoutePattern.RawText);

        // 1. ¿La acción está registrada?
        var permiso = await autorizador.BuscarPermisoAsync(metodo, ruta, ct);
        if (permiso is null)
        {
            await Rechazar(http, StatusCodes.Status404NotFound,
                "permiso.no_disponible", "La acción no está disponible, contacte con un administrador.");
            return;
        }

        // 2 y 3. El alcance debe corresponder al tipo de sesión, y el rol debe tener el permiso.
        var permitido = (tenant.TipoSesion, permiso.Alcance) switch
        {
            (TipoSesion.Empresa, AlcancePermiso.Empresa) when tenant.VinculoEmpresaId is Guid vinculo =>
                await autorizador.VinculoTienePermisoAsync(vinculo, permiso.Id, ct),
            (TipoSesion.Plataforma, AlcancePermiso.Plataforma) when AccesoId(http.User) is Guid acceso =>
                await autorizador.StaffTienePermisoAsync(acceso, permiso.Id, ct),
            _ => false
        };

        if (!permitido)
        {
            await Rechazar(http, StatusCodes.Status403Forbidden,
                "permiso.prohibido", "No tiene permiso para realizar esta acción.");
            return;
        }

        await next(http); // recién aquí se ejecuta el controlador
    }

    private static Guid? AccesoId(ClaimsPrincipal u) =>
        Guid.TryParse((u.FindFirst(JwtRegisteredClaimNames.Sub) ?? u.FindFirst(ClaimTypes.NameIdentifier))?.Value, out var id)
            ? id : null;

    private static Task Rechazar(HttpContext http, int status, string codigo, string mensaje)
    {
        var problema = new ProblemDetails { Status = status, Title = mensaje, Instance = http.Request.Path };
        problema.Extensions["codigo"] = codigo;
        http.Response.StatusCode = status;
        return http.Response.WriteAsJsonAsync(problema, problema.GetType(), options: null, contentType: "application/problem+json");
    }
}