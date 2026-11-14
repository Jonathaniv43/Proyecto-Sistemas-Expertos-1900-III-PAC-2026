using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Norvia.API.Middleware;

/// <summary>
/// Middleware global de errores. Intercepta la <see cref="ValidationException"/> de FluentValidation
/// y la devuelve como 400 ProblemDetails (RFC 7807) con los errores por campo. Cualquier otra
/// excepción responde 500 sin detalles internos.
/// </summary>
public sealed class ManejoErroresMiddleware(RequestDelegate next, ILogger<ManejoErroresMiddleware> log)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var errores = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            await EscribirAsync(context, new ValidationProblemDetails(errores)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "La solicitud tiene datos inválidos."
            });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);
            await EscribirAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Ocurrió un error inesperado."
            });
        }
    }

    private static Task EscribirAsync(HttpContext context, ProblemDetails problema)
    {
        problema.Instance = context.Request.Path;
        context.Response.StatusCode = problema.Status!.Value;
        return context.Response.WriteAsJsonAsync(problema, problema.GetType(), options: null, contentType: "application/problem+json");
    }
}
