using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.DTOs;

/// <summary>
/// Medida correctiva de un evento (HU-050): una tarea del plan de trabajo con origen en el evento (RN-23, D6). Su avance
/// es el de la tarea, que se trabaja desde el plan (HU-044).
/// </summary>
public sealed record MedidaCorrectivaResponseDto(
    Guid Id,
    string Titulo,
    string? Descripcion,
    EstadoTarea Estado,
    short Prioridad,
    Guid? ResponsableId,
    string? ResponsableNombre,
    DateOnly? FechaLimite,
    bool RequiereEvidencia,
    DateTime CreadaEn,
    DateTime? CompletadaEn);
