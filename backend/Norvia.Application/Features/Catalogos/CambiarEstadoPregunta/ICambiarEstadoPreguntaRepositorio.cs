using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPregunta;

public interface ICambiarEstadoPreguntaRepositorio
{
    Task<PreguntaExploratoria?> BuscarAsync(Guid id, CancellationToken ct);
}
