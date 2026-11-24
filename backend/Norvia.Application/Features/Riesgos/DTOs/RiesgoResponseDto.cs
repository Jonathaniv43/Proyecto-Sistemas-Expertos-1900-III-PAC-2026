using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.DTOs;

/// <summary>
/// Riesgo de la empresa (HU-036). <paramref name="PeligroNombre"/> viene del catálogo; si el peligro no está en
/// él, <paramref name="DescripcionPeligro"/> lo describe. <paramref name="EncuestaId"/> y
/// <paramref name="EncuestaTitulo"/> indican la encuesta que lo sugirió (HU-035); sin encuesta, se agregó a mano.
/// </summary>
public sealed record RiesgoResponseDto(
    Guid Id,
    EstadoRiesgo Estado,
    Guid? PeligroId,
    string? PeligroNombre,
    string? DescripcionPeligro,
    string? Actividad,
    Guid? EncuestaId,
    string? EncuestaTitulo,
    DateTime CreadoEn);
