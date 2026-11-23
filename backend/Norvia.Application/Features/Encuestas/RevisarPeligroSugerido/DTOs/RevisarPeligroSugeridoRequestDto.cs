using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.RevisarPeligroSugerido.DTOs;

/// <summary>Decisión sobre un peligro sugerido por la encuesta (HU-035).</summary>
public sealed record RevisarPeligroSugeridoRequestDto
{
    /// <summary><c>confirmado</c> (pasa a la matriz) o <c>descartado</c> (no aplica).</summary>
    public EstadoRiesgo? Estado { get; init; }

    /// <summary>Opcional al confirmar: actividad o lugar donde se presenta el riesgo. No aplica al descartar.</summary>
    public string? Actividad { get; init; }
}
