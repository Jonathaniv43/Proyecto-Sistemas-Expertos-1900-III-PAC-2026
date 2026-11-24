namespace Norvia.Application.Features.Plan.AsignarTarea.DTOs;

/// <summary>Asignar una tarea (HU-043): responsable y fecha límite, los dos obligatorios (RN-22).</summary>
public sealed record AsignarTareaRequestDto
{
    public Guid? ResponsableId { get; init; }
    public DateOnly? FechaLimite { get; init; }
}
