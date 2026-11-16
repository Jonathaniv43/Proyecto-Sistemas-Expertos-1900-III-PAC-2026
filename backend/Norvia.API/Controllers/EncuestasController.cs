using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles;
using Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles.DTOs;

namespace Norvia.API.Controllers;

/// <summary>Módulo Encuestas (M3). Cada acción usa solo el servicio de su caso de uso.</summary>
[ApiController]
[Route("api/encuestas")]
public sealed class EncuestasController : ControllerBase
{
    /// <summary>HU-029: categorías del rubro de la empresa, con la vista previa de sus preguntas.</summary>
    [HttpGet("categorias-disponibles")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDisponibleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ListarCategoriasDisponibles(
        [FromServices] ListarCategoriasDisponiblesServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(cancellationToken)).AHttp(this);
}
