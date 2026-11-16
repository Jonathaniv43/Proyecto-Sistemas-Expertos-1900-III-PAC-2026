using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta;

public interface IActualizarBorradorEncuestaRepositorio
{
    /// <remarks>Incluye Versiones con sus Categorias, Preguntas y Opciones.</remarks>
    Task<Encuesta?> BuscarConVersionesAsync(Guid encuestaId, CancellationToken ct);

    Task<bool> ExistenPeligrosActivosAsync(IReadOnlyCollection<Guid> peligroIds, CancellationToken ct);

    void AgregarVersion(EncuestaVersion version);
    void ReemplazarPreguntas(EncuestaVersion borrador, IEnumerable<EncuestaPregunta> nuevasPreguntas);
}
