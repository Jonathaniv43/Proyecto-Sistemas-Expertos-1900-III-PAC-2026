using Norvia.Domain.Entities.Accidentes;

namespace Norvia.Application.Features.Eventos.ObtenerEvento;

public interface IObtenerEventoRepositorio
{
    Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct);
}
