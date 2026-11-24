namespace Norvia.Application.Features.Riesgos.EvaluarRiesgoResidual.DTOs;

/// <summary>Evaluación residual de un riesgo (HU-040): probabilidad y severidad del 1 al 5 con los controles aplicados (RN-15).</summary>
public sealed record EvaluarRiesgoResidualRequestDto
{
    public short? Probabilidad { get; init; }
    public short? Severidad { get; init; }
}
