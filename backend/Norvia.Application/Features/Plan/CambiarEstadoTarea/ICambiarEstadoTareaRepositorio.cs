using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Plan;

namespace Norvia.Application.Features.Plan.CambiarEstadoTarea;

public interface ICambiarEstadoTareaRepositorio
{
    /// <summary>La tarea de la empresa de la sesión, con seguimiento para modificarla; null si no existe.</summary>
    Task<Tarea?> BuscarAsync(Guid tareaId, CancellationToken ct);

    /// <summary>Rol actual del vínculo (de la base, no del token: refleja un cambio de rol reciente).</summary>
    Task<short> RolDelVinculoAsync(Guid vinculoId, CancellationToken ct);

    /// <summary>Cuántas evidencias tiene la tarea (RN-25). Hasta HU-045 no hay dónde adjuntarlas.</summary>
    Task<int> EvidenciasAsync(Guid tareaId, CancellationToken ct);

    Task<TareaResponseDto> ProyectarAsync(Guid tareaId, CancellationToken ct);
}
