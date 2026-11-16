using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.ObtenerEncuesta;

public interface IObtenerEncuestaRepositorio
{
    /// <remarks>Incluye Versiones (publicada y borrador), Preguntas y Opciones.</remarks>
    Task<Encuesta?> BuscarAsync(Guid id, CancellationToken ct);
}
