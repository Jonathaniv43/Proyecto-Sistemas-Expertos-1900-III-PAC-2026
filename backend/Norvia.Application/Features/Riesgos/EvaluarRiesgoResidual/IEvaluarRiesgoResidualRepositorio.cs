using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.EvaluarRiesgoResidual;

public interface IEvaluarRiesgoResidualRepositorio
{
    /// <summary>Estado del riesgo de la empresa de la sesión, si ya tiene evaluación inicial y si tiene controles; null si no existe.</summary>
    Task<(EstadoRiesgo Estado, bool TieneInicial, bool TieneControles)?> RiesgoAsync(Guid riesgoId, CancellationToken ct);

    /// <summary>Nombre completo de la persona del vínculo (quien evalúa).</summary>
    Task<string> NombreDelVinculoAsync(Guid vinculoId, CancellationToken ct);

    void Agregar(EvaluacionRiesgo evaluacion);
}
