using Microsoft.AspNetCore.Mvc;
using Norvia.API.Extensiones;
using Norvia.Application.Features.Plan.AsignarTarea;
using Norvia.Application.Features.Plan.AsignarTarea.DTOs;
using Norvia.Application.Features.Plan.CrearTareaManual;
using Norvia.Application.Features.Plan.CrearTareaManual.DTOs;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Application.Features.Plan.GenerarPlan;
using Norvia.Application.Features.Plan.GenerarPlan.DTOs;
using Norvia.Application.Features.Plan.ListarTareas;
using Norvia.Application.Features.Plan.ListarResponsables;
using Norvia.Application.Features.Plan.ListarResponsables.DTOs;
using Norvia.Application.Features.Plan.ObtenerPropuestaPlan;
using Norvia.Application.Features.Plan.ObtenerPropuestaPlan.DTOs;

namespace Norvia.API.Controllers;

/// <summary>Módulo Plan de trabajo (M5): las tareas de la empresa de la sesión.</summary>
[ApiController]
[Route("api/plan-trabajo")]
public sealed class PlanTrabajoController : ControllerBase
{
    /// <summary>HU-042: tareas que se agregarían al plan a partir de los riesgos y sus controles, sin guardar nada.</summary>
    [HttpGet("propuesta")]
    [ProducesResponseType(typeof(PropuestaPlanResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Propuesta([FromServices] ObtenerPropuestaPlanServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    /// <summary>HU-042: confirma el plan con las tareas elegidas de la propuesta. Repetirlo no duplica tareas.</summary>
    [HttpPost("generacion")]
    [ProducesResponseType(typeof(GenerarPlanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generar(
        GenerarPlanRequestDto request, [FromServices] GenerarPlanServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttp(this);

    /// <summary>HU-042: tareas del plan, por prioridad y nivel del riesgo.</summary>
    [HttpGet("tareas")]
    [ProducesResponseType(typeof(IReadOnlyList<TareaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Tareas([FromServices] ListarTareasServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));

    /// <summary>HU-043: crea una tarea manual, ya asignada (responsable y fecha límite).</summary>
    [HttpPost("tareas")]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearTarea(
        CrearTareaManualRequestDto request, [FromServices] CrearTareaManualServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(request, cancellationToken)).AHttpCreado(this);

    /// <summary>HU-043: asigna o reasigna el responsable y la fecha límite de una tarea.</summary>
    [HttpPut("tareas/{id:guid}/asignacion")]
    [ProducesResponseType(typeof(TareaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Asignar(
        Guid id, AsignarTareaRequestDto request, [FromServices] AsignarTareaServicio servicio, CancellationToken cancellationToken) =>
        (await servicio.EjecutarAsync(id, request, cancellationToken)).AHttp(this);

    /// <summary>HU-043: a quién se le puede asignar una tarea.</summary>
    [HttpGet("responsables")]
    [ProducesResponseType(typeof(IReadOnlyList<ResponsableResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Responsables([FromServices] ListarResponsablesServicio servicio, CancellationToken cancellationToken) =>
        Ok(await servicio.EjecutarAsync(cancellationToken));
}
