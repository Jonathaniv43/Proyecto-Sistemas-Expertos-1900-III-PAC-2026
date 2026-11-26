using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Encuestas.ListarTiposRespuesta;

public interface IListarTiposRespuestaRepositorio
{
    Task<List<TipoRespuestaPreguntaCatalogo>> ListarActivosAsync(CancellationToken ct);
}
