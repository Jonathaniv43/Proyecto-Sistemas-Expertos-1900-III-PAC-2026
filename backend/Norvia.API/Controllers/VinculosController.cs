using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Vinculos.CrearVinculo;
using Norvia.Application.Features.Vinculos.CrearVinculo.DTOs;

namespace Norvia.API.Controllers;

/// <summary>Módulo Vinculos: acceso de las personas a la empresa de la sesión.</summary>
[ApiController]
[Route("api/vinculos")]
public sealed class VinculosController : ControllerBase
{
    /// <summary>HU-020: agrega una persona a la empresa con un rol, creando o reutilizando su cuenta.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CrearVinculoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(
        CrearVinculoRequestDto request, [FromServices] CrearVinculoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);
}
