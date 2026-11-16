using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.PublicarVersionEncuesta;

public interface IPublicarVersionEncuestaRepositorio
{
    /// <remarks>Incluye Versiones y sus Preguntas.</remarks>
    Task<Encuesta?> BuscarConVersionesAsync(Guid encuestaId, CancellationToken ct);
}
