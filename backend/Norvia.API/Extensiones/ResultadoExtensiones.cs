using Microsoft.AspNetCore.Mvc;
using Norvia.Application.Common.Resultados;

namespace Norvia.API.Extensiones;

/// <summary>Traduce el <see cref="Resultado"/> / <see cref="Error"/> de Application a HTTP (RFC 7807).</summary>
public static class ResultadoExtensiones
{
    public static IActionResult AHttp<T>(this Resultado<T> resultado, ControllerBase controller) =>
        resultado.EsExito ? controller.Ok(resultado.Valor) : Problema(resultado.Error!, controller);

    /// <summary>Como <see cref="AHttp{T}"/>, pero responde 201 Created cuando el caso de uso crea un recurso.</summary>
    public static IActionResult AHttpCreado<T>(this Resultado<T> resultado, ControllerBase controller) =>
        resultado.EsExito
            ? controller.StatusCode(StatusCodes.Status201Created, resultado.Valor)
            : Problema(resultado.Error!, controller);

    /// <summary>Como <see cref="AHttp{T}"/>, pero responde 202 Accepted (p. ej. reenvío de correo).</summary>
    public static IActionResult AHttpAceptado<T>(this Resultado<T> resultado, ControllerBase controller) =>
        resultado.EsExito
            ? controller.Accepted(resultado.Valor)
            : Problema(resultado.Error!, controller);

    public static IActionResult AHttp(this Resultado resultado, ControllerBase controller) =>
        resultado.EsExito ? controller.NoContent() : Problema(resultado.Error!, controller);

    /// <summary>Escribe problem+json en el pipeline (middleware) con el mismo mapa que <see cref="AHttp"/>.</summary>
    public static Task EscribirProblemaAsync(HttpContext http, Error error)
    {
        var status = StatusDe(error);
        var problema = CrearProblema(error, status, http.Request.Path);
        http.Response.StatusCode = status;
        return http.Response.WriteAsJsonAsync(problema, problema.GetType(), options: null, contentType: "application/problem+json");
    }

    public static int StatusDe(Error error) => error.Tipo switch
    {
        TipoError.Validacion => StatusCodes.Status400BadRequest,
        TipoError.NoAutorizado => StatusCodes.Status401Unauthorized,
        TipoError.Prohibido => StatusCodes.Status403Forbidden,
        TipoError.NoEncontrado => StatusCodes.Status404NotFound,
        TipoError.Conflicto => StatusCodes.Status409Conflict,
        TipoError.Servidor => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };

    private static IActionResult Problema(Error error, ControllerBase controller)
    {
        var status = StatusDe(error);
        var problema = CrearProblema(error, status, controller.HttpContext.Request.Path);
        return new ObjectResult(problema) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }

    private static ProblemDetails CrearProblema(Error error, int status, PathString instance)
    {
        var problema = new ProblemDetails
        {
            Status = status,
            Title = error.Mensaje,
            Instance = instance
        };
        problema.Extensions["codigo"] = error.Codigo;
        return problema;
    }
}
