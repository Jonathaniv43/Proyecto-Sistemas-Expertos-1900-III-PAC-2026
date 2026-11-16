using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.CerrarEncuesta;

public interface ICerrarEncuestaRepositorio
{
    Task<Encuesta?> BuscarAsync(Guid encuestaId, CancellationToken ct);
}
