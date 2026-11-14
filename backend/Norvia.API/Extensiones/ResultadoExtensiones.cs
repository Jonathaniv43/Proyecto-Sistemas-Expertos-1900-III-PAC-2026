using Microsoft.AspNetCore.Mvc;
using Norvia.Application.Common.Resultados;

namespace Norvia.API.Extensiones;

/// <summary>Traduce el <see cref="Resultado"/> de un caso de uso a una respuesta HTTP.</summary>
public static class ResultadoExtensiones
{
    public static IActionResult AHttp<T>(this Resultado<T> resultado, ControllerBase controller) =>
        resultado.EsExito ? controller.Ok(resultado.Valor) : Problema(resultado.Error!, controller);

    public static IActionResult AHttp(this Resultado resultado, ControllerBase controller) =>
        resultado.EsExito ? controller.NoContent() : Problema(resultado.Error!, controller);

    private static IActionResult Problema(Error error, ControllerBase controller)
    {
        var status = error.Tipo switch
        {
            TipoError.Validacion => StatusCodes.Status400BadRequest,
            TipoError.NoAutorizado => StatusCodes.Status401Unauthorized,
            TipoError.Prohibido => StatusCodes.Status403Forbidden,
            TipoError.NoEncontrado => StatusCodes.Status404NotFound,
            TipoError.Conflicto => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problema = new ProblemDetails
        {
            Status = status,
            Title = error.Mensaje,
            Instance = controller.HttpContext.Request.Path
        };
        problema.Extensions["codigo"] = error.Codigo;

        return new ObjectResult(problema) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }
}
