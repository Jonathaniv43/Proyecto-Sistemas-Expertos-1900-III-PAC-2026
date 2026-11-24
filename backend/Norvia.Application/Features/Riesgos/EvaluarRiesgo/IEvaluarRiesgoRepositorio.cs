using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.EvaluarRiesgo;

public interface IEvaluarRiesgoRepositorio
{
    /// <summary>Estado del riesgo de la empresa de la sesión, o null si no existe.</summary>
    Task<EstadoRiesgo?> EstadoDelRiesgoAsync(Guid riesgoId, CancellationToken ct);

    /// <summary>Nombre completo de la persona del vínculo (quien evalúa).</summary>
    Task<string> NombreDelVinculoAsync(Guid vinculoId, CancellationToken ct);

    /// <summary>true si el evento es de la empresa de la sesión y no está descartado (HU-051).</summary>
    Task<bool> EventoEnlazableAsync(Guid accidenteId, CancellationToken ct);

    void Agregar(EvaluacionRiesgo evaluacion);
}
