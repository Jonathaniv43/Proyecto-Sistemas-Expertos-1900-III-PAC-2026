namespace Norvia.Application.Features.Plan.GenerarPlan.DTOs;

/// <summary>
/// Confirmar el plan (HU-042): las claves de las tareas propuestas que se agregan. Las que se desmarcaron no se envían
/// y vuelven a proponerse la próxima vez.
/// </summary>
public sealed record GenerarPlanRequestDto
{
    public IReadOnlyList<string>? Claves { get; init; }
}

/// <summary>Cuántas tareas se agregaron. Una clave que ya tenía tarea no se repite (0 si todo ya estaba).</summary>
public sealed record GenerarPlanResponseDto(int TareasCreadas);
