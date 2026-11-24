using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Domain.Entities.Plan;

namespace Norvia.Application.Features.Eventos.CrearMedidaCorrectiva;

public interface ICrearMedidaCorrectivaRepositorio
{
    /// <summary>El evento de la empresa de la sesión (sin seguimiento); null si no existe.</summary>
    Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct);

    /// <summary>true si el vínculo es de la empresa de la sesión, está activo y su rol puede tener tareas.</summary>
    Task<bool> EsResponsableValidoAsync(Guid vinculoId, CancellationToken ct);

    void Agregar(Tarea tarea);

    Task<MedidaCorrectivaResponseDto> ProyectarAsync(Guid tareaId, CancellationToken ct);
}
