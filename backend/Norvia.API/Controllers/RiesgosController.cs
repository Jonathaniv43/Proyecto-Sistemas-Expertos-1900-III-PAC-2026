using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Riesgos.CrearRiesgo;
using Norvia.Application.Features.Riesgos.CrearRiesgo.DTOs;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.EvaluarRiesgo;
using Norvia.Application.Features.Riesgos.EvaluarRiesgo.DTOs;
using Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles;
using Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles.DTOs;
using Norvia.Application.Features.Riesgos.ListarRiesgos;
using Norvia.Application.Features.Riesgos.ListarRiesgos.DTOs;
using Norvia.Application.Features.Riesgos.ObtenerMatriz;
using Norvia.Application.Features.Riesgos.ObtenerMatriz.DTOs;
using Norvia.Application.Features.Riesgos.ObtenerRiesgo;

namespace Norvia.API.Controllers;

/// <summary>Módulo Riesgos (M4): la matriz de riesgos de la empresa de la sesión.</summary>
[ApiController]
[Route("api/riesgos")]
public sealed class RiesgosController : ControllerBase
{
    /// <summary>
    /// HU-036, HU-038: riesgos de la empresa con filtros (estado, texto, clasificación del peligro, nivel, celda).
    /// Sin estado, los de la matriz. Nivel más alto primero.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RiesgoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery] FiltrosRiesgosDto filtros, [FromServices] ListarRiesgosServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(filtros, cancellationToken));

    /// <summary>HU-038: cuántos riesgos de la matriz hay en cada celda de probabilidad × severidad.</summary>
    [HttpGet("matriz")]
    [ProducesResponseType(typeof(MatrizRiesgosResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Matriz([FromServices] ObtenerMatrizServicio servicio, CancellationToken cancellationToken) =>
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

    /// <summary>HU-037: evaluación inicial (probabilidad × severidad); se suma al historial del riesgo.</summary>
    [HttpPost("{id:guid}/evaluaciones")]
    [ProducesResponseType(typeof(EvaluacionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Evaluar(
        Guid id, EvaluarRiesgoRequestDto request, [FromServices] EvaluarRiesgoServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttpCreado(this);
}
