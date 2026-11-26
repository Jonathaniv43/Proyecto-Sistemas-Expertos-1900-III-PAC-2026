namespace Norvia.Application.Features.Plan.CrearTareaManual.DTOs;

/// <summary>
/// Tarea manual (HU-043): sin riesgo ni accidente de origen (RN-23). Nace asignada: responsable y fecha límite son
/// obligatorios, igual que al asignar una tarea del plan.
/// </summary>
public sealed record CrearTareaManualRequestDto
{
    public string? Titulo { get; init; }
    public string? Descripcion { get; init; }

    /// <summary>1 alta, 2 media, 3 baja.</summary>
    public short? Prioridad { get; init; }
    public Guid? ResponsableId { get; init; }
    public DateOnly? FechaLimite { get; init; }
    public bool RequiereEvidencia { get; init; }
}
