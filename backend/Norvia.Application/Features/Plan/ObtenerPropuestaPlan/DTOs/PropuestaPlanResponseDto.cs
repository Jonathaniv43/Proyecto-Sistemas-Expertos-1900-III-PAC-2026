using Norvia.Application.Features.Plan.Propuesta;

namespace Norvia.Application.Features.Plan.ObtenerPropuestaPlan.DTOs;

/// <summary>
/// Vista previa del plan (HU-042): las tareas que se agregarían, en orden de prioridad, sin guardar nada.
/// <paramref name="RiesgosSinEvaluar"/> son riesgos de la matriz que no entran porque no tienen nivel.
/// </summary>
public sealed record PropuestaPlanResponseDto(IReadOnlyList<TareaPropuesta> Tareas, int RiesgosSinEvaluar);
