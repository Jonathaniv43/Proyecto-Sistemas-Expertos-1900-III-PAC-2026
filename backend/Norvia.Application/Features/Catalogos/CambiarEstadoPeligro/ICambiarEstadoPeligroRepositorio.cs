using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPeligro;

public interface ICambiarEstadoPeligroRepositorio
{
    Task<Peligro?> BuscarAsync(Guid id, CancellationToken ct);
}
