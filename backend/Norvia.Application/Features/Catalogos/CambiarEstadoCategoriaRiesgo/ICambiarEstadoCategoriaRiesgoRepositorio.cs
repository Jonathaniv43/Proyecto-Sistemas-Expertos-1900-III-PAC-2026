using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo;

public interface ICambiarEstadoCategoriaRiesgoRepositorio
{
    Task<CategoriaRiesgo?> BuscarAsync(Guid id, CancellationToken ct);
}
