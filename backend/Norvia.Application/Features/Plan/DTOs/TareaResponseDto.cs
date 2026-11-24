using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.DTOs;

/// <summary>Origen de una tarea (RN-23): se deduce de qué columna de origen tiene valor.</summary>
public enum OrigenTarea
{
    Riesgo,
    Accidente,
    Manual
}

/// <summary>
/// Tarea del plan (HU-042). Con origen riesgo trae su nombre y su nivel inicial actual; si implementa un control, su tipo.
/// Responsable y fecha límite se asignan en HU-043.
/// </summary>
public sealed record TareaResponseDto(
    Guid Id,
    string Titulo,
    string? Descripcion,
    EstadoTarea Estado,
    short Prioridad,
    Guid? ResponsableId,
    string? ResponsableNombre,
    DateOnly? FechaLimite,
    bool RequiereEvidencia,
    OrigenTarea Origen,
    Guid? RiesgoId,
    string? RiesgoNombre,
    int? RiesgoNivel,
    ClasificacionRiesgo? RiesgoClasificacion,
    TipoControl? ControlTipo,
    DateTime CreadaEn);
