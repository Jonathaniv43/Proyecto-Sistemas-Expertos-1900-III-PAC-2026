using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ListarRubros;

public interface IListarRubrosRepositorio
{
    Task<List<Rubro>> ListarAsync(CancellationToken ct);
}
