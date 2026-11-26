using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.DTOs;

/// <summary>
/// Evaluación de un riesgo (HU-037). <paramref name="Nivel"/> = probabilidad × severidad (RN-15) y
/// <paramref name="Clasificacion"/> se calculan al responder; no se guardan (RN-16). <paramref name="AccidenteId"/>: el evento a raíz
/// del cual se hizo (HU-051); null si no.
/// </summary>
public sealed record EvaluacionResponseDto(
    Guid Id,
    TipoEvaluacion Tipo,
    short Probabilidad,
    short Severidad,
    int Nivel,
    ClasificacionRiesgo Clasificacion,
    Guid EvaluadoPorId,
    string EvaluadoPorNombre,
    DateTime EvaluadoEn,
    Guid? AccidenteId)
{
    public static EvaluacionResponseDto De(EvaluacionRiesgo e, string evaluadoPorNombre) =>
        new(e.Id, e.Tipo, e.Probabilidad, e.Severidad, e.Nivel, e.Clasificacion, e.EvaluadoPorId, evaluadoPorNombre, e.EvaluadoEn, e.AccidenteId);
}
