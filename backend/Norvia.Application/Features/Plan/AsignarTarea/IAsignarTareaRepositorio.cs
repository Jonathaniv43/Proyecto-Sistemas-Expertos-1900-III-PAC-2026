using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Plan;

namespace Norvia.Application.Features.Plan.AsignarTarea;

public interface IAsignarTareaRepositorio
{
    /// <summary>La tarea de la empresa de la sesión, con seguimiento para modificarla; null si no existe.</summary>
    Task<Tarea?> BuscarAsync(Guid tareaId, CancellationToken ct);

    /// <summary>true si el vínculo es de la empresa de la sesión, está activo y su rol puede tener tareas.</summary>
    Task<bool> EsResponsableValidoAsync(Guid vinculoId, CancellationToken ct);

    Task<TareaResponseDto> ProyectarAsync(Guid tareaId, CancellationToken ct);
}
