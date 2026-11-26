using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta;

public interface IObtenerReporteEncuestaRepositorio
{
    Task<Encuesta?> BuscarConVersionAsync(Guid encuestaId, Guid? versionId, CancellationToken ct);
    Task<int> ContarParticipacionesAsync(Guid encuestaId, CancellationToken ct);
    Task<List<RespuestaEncuesta>> ListarRespuestasDeVersionAsync(Guid versionId, CancellationToken ct);
}
