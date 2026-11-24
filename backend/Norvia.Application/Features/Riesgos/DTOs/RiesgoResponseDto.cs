using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.DTOs;

/// <summary>
/// Riesgo de la empresa (HU-036). <paramref name="PeligroNombre"/> viene del catálogo; si el peligro no está en
/// él, <paramref name="DescripcionPeligro"/> lo describe y no hay <paramref name="PeligroClasificacion"/>. <paramref name="EncuestaId"/> y
/// <paramref name="EncuestaTitulo"/> indican la encuesta que lo sugirió (HU-035); sin encuesta, se agregó a mano.
/// <paramref name="Evaluacion"/> es la última evaluación inicial (HU-037); null si todavía no se evaluó.
/// <paramref name="TotalControles"/> cuenta sus controles (HU-039): un riesgo alto o crítico sin controles se señala (RN-19).
/// </summary>
public sealed record RiesgoResponseDto(
    Guid Id,
    EstadoRiesgo Estado,
    Guid? PeligroId,
    string? PeligroNombre,
    ClasificacionPeligro? PeligroClasificacion,
    string? DescripcionPeligro,
    string? Actividad,
    Guid? EncuestaId,
    string? EncuestaTitulo,
    DateTime CreadoEn,
    EvaluacionResponseDto? Evaluacion,
    int TotalControles);
