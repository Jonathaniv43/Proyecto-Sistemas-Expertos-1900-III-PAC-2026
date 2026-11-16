using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Norvia.API.Extensiones;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Features.AccesoRecurso;
using Norvia.Application.Features.AccesoRecurso.PermitirAccesoRecurso;

namespace Norvia.API.Middleware;

public sealed class AutorizacionPermisosMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http, ITenantActual tenant, PermitirAccesoRecursoServicio servicio)
    {
        var ctx = ExtraerContexto(http);
        var error = await servicio.EvaluarAsync(ctx, tenant, http.RequestAborted);
        if (error is null)
        {
            await next(http);
            return;
        }

        // Bloquea antes del controller; el status HTTP lo define ResultadoExtensiones (igual que AHttp).
        await ResultadoExtensiones.EscribirProblemaAsync(http, error);
    }

    private static ContextoAccesoRecurso ExtraerContexto(HttpContext http)
    {
        if (http.GetEndpoint() is not RouteEndpoint endpoint
            || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            return new ContextoAccesoRecurso(OmitirVerificacion: true, false, "", "", null);

        return new ContextoAccesoRecurso(
            OmitirVerificacion: false,
            EstaAutenticado: http.User.Identity?.IsAuthenticated == true,
            MetodoHttp: http.Request.Method.ToUpperInvariant(),
            RutaPatron: RutaNormalizada.De(endpoint.RoutePattern.RawText),
            AccesoId: AccesoId(http.User));
    }

    private static Guid? AccesoId(ClaimsPrincipal u) =>
        Guid.TryParse((u.FindFirst(JwtRegisteredClaimNames.Sub) ?? u.FindFirst(ClaimTypes.NameIdentifier))?.Value, out var id)
            ? id : null;
}
