using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Accidentes;

namespace Norvia.Application.Features.Eventos.DescartarEvento;

public interface IDescartarEventoRepositorio
{
    /// <summary>El evento de la empresa de la sesión (con seguimiento); null si no existe.</summary>
    Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct);

    Task<EventoDetalleResponseDto> DetalleAsync(Guid id, CancellationToken ct);
}
