using Norvia.Application.Features.Riesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarEvaluaciones;

public interface IListarEvaluacionesRepositorio
{
    Task<bool> ExisteRiesgoAsync(Guid riesgoId, CancellationToken ct);

    /// <summary>Todas las evaluaciones del riesgo (iniciales y residuales) con su evaluador, la más reciente primero.</summary>
    Task<IReadOnlyList<EvaluacionResponseDto>> ListarAsync(Guid riesgoId, CancellationToken ct);
}
