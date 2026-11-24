namespace Norvia.Application.Features.Riesgos.EvaluarRiesgo.DTOs;

/// <summary>Evaluación inicial de un riesgo (HU-037): probabilidad y severidad del 1 al 5 (RN-15).</summary>
public sealed record EvaluarRiesgoRequestDto
{
    public short? Probabilidad { get; init; }
    public short? Severidad { get; init; }

    /// <summary>HU-051: el evento a raíz del cual se reevalúa, si lo hay.</summary>
    public Guid? AccidenteId { get; init; }
}
