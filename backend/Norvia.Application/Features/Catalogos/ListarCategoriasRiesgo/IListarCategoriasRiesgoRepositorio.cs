using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ListarCategoriasRiesgo;

public interface IListarCategoriasRiesgoRepositorio
{
    Task<List<CategoriaRiesgo>> ListarAsync(short? rubroId, CancellationToken ct);
}
