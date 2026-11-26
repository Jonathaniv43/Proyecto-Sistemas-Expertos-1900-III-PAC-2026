using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.CompararEncuestas;

public interface ICompararEncuestasRepositorio
{
    Task<EncuestaVersion?> BuscarVersionAsync(Guid versionId, CancellationToken ct);
    Task<int> ContarParticipacionesAsync(Guid encuestaId, CancellationToken ct);
    Task<List<RespuestaEncuesta>> ListarRespuestasDeVersionAsync(Guid versionId, CancellationToken ct);
    Task<Dictionary<Guid, EstadoRiesgo>> EstadosRiesgosPorPeligroAsync(Guid encuestaId, CancellationToken ct);
}
