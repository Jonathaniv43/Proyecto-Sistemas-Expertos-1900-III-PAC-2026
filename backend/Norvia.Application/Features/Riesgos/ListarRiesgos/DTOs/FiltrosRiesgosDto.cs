using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.ListarRiesgos.DTOs;

/// <summary>
/// Filtros de la lista de riesgos (HU-038), como parámetros de la consulta. Todos son opcionales.
/// El nivel y la celda se refieren a la última evaluación inicial (HU-037).
/// </summary>
public sealed record FiltrosRiesgosDto
{
    /// <summary><c>confirmado</c> (la matriz, por defecto), <c>sugerido</c> (por revisar) o <c>descartado</c>.</summary>
    public EstadoRiesgo? Estado { get; init; }

    /// <summary>Texto en el nombre o la descripción del peligro, o en la actividad (sin distinguir mayúsculas).</summary>
    public string? Buscar { get; init; }

    /// <summary>Clasificación del peligro del catálogo; los peligros descritos por la empresa no tienen.</summary>
    public ClasificacionPeligro? ClasificacionPeligro { get; init; }

    /// <summary>Clasificación del nivel de riesgo (RN-16).</summary>
    public ClasificacionRiesgo? Nivel { get; init; }

    /// <summary>true: solo los que todavía no tienen evaluación.</summary>
    public bool? SinEvaluar { get; init; }

    /// <summary>true: solo los altos o críticos que todavía no tienen controles (RN-19, HU-039).</summary>
    public bool? SinControl { get; init; }

    /// <summary>Celda del mapa: se envían las dos o ninguna.</summary>
    public short? Probabilidad { get; init; }
    public short? Severidad { get; init; }
}
