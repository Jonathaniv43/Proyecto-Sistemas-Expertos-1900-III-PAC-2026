using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ListarRiesgosDelEvento.DTOs;

/// <summary>Cómo se enlaza un riesgo con un evento (HU-051).</summary>
public enum RelacionRiesgoEvento
{
    /// <summary>El evento reveló el riesgo: se agregó a la matriz a raíz de él.</summary>
    Revelado,

    /// <summary>El riesgo ya estaba en la matriz y se reevaluó a raíz del evento.</summary>
    Reevaluado
}

/// <summary>
/// Riesgo de la matriz enlazado con un evento (HU-051). Revelado: <paramref name="Nivel"/> es su nivel inicial actual
/// (null si todavía no se evalúa). Reevaluado: <paramref name="Nivel"/> es el de la evaluación hecha a raíz del evento y
/// <paramref name="NivelAnterior"/> el de la evaluación inicial previa (null si no había). El nivel es P × S (RN-15) y la
/// clasificación sale de él (RN-16).
/// </summary>
public sealed record RiesgoDelEventoResponseDto(
    Guid RiesgoId,
    string Nombre,
    string? Actividad,
    RelacionRiesgoEvento Relacion,
    int? NivelAnterior,
    ClasificacionRiesgo? ClasificacionAnterior,
    int? Nivel,
    ClasificacionRiesgo? Clasificacion,
    DateTime? EvaluadoEn);
