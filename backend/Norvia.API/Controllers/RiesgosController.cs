using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Riesgos.CrearRiesgo;
using Norvia.Application.Features.Riesgos.CrearRiesgo.DTOs;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles;
using Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles.DTOs;
using Norvia.Application.Features.Riesgos.ListarRiesgos;
using Norvia.Application.Features.Riesgos.ObtenerRiesgo;

namespace Norvia.API.Controllers;

/// <summary>Módulo Riesgos (M4): la matriz de riesgos de la empresa de la sesión.</summary>
[ApiController]
[Route("api/riesgos")]
public sealed class RiesgosController : ControllerBase
{
    /// <summary>HU-036: riesgos de la matriz (confirmados), los más recientes primero.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RiesgoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromServices] ListarRiesgosServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    /// <summary>HU-036: peligros activos del catálogo, para elegir al agregar un riesgo.</summary>
    [HttpGet("peligros-disponibles")]
    [ProducesResponseType(typeof(IReadOnlyList<PeligroDisponibleResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPeligrosDisponibles(
        [FromServices] ListarPeligrosDisponiblesServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RiesgoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener(Guid id, [FromServices] ObtenerRiesgoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, cancellationToken)).AHttp(this);

    /// <summary>HU-036: agrega a la matriz un riesgo que la encuesta no detectó.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RiesgoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(
        CrearRiesgoRequestDto request, [FromServices] CrearRiesgoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);
}
