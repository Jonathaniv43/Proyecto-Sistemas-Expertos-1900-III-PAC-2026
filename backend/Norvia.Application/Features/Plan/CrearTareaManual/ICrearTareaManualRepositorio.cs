using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Plan;

namespace Norvia.Application.Features.Plan.CrearTareaManual;

public interface ICrearTareaManualRepositorio
{
    /// <summary>true si el vínculo es de la empresa de la sesión, está activo y su rol puede tener tareas.</summary>
    Task<bool> EsResponsableValidoAsync(Guid vinculoId, CancellationToken ct);

    void Agregar(Tarea tarea);

    Task<TareaResponseDto> ProyectarAsync(Guid tareaId, CancellationToken ct);
}
