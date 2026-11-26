using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ActualizarPeligro;

public interface IActualizarPeligroRepositorio
{
    Task<Peligro?> BuscarAsync(Guid id, CancellationToken ct);
}
