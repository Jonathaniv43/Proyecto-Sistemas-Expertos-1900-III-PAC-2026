using Norvia.Domain.Entities.Encuestas;

namespace Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta;

public interface IObtenerReporteEncuestaRepositorio
{
    Task<Encuesta?> BuscarConVersionPublicadaAsync(Guid encuestaId, CancellationToken ct);
    Task<int> ContarParticipacionesAsync(Guid encuestaId, CancellationToken ct);
    Task<List<RespuestaEncuesta>> ListarRespuestasDeVersionAsync(Guid versionId, CancellationToken ct);
}
