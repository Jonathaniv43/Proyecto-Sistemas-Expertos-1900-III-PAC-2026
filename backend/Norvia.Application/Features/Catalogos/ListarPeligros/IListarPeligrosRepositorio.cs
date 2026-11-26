using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ListarPeligros;

public interface IListarPeligrosRepositorio
{
    Task<List<Peligro>> ListarAsync(CancellationToken ct);
}
