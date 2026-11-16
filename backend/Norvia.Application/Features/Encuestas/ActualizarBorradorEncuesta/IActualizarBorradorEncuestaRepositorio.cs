using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta;

public interface IActualizarBorradorEncuestaRepositorio
{
    Task<Encuesta?> BuscarConVersionesAsync(Guid encuestaId, CancellationToken ct);
    Task<bool> ExistenPeligrosActivosAsync(IReadOnlyCollection<Guid> peligroIds, CancellationToken ct);
    Task<IReadOnlyDictionary<string, TipoRespuestaPreguntaCatalogo>> TiposPorCodigoAsync(CancellationToken ct);
    void AgregarVersion(EncuestaVersion version);
    void ReemplazarSecciones(EncuestaVersion borrador, IEnumerable<EncuestaSeccion> nuevasSecciones);
    void ReemplazarPreguntas(EncuestaVersion borrador, IEnumerable<EncuestaPregunta> nuevasPreguntas);
}
