using Norvia.Application.Features.Eventos.ListarRiesgosDelEvento.DTOs;

namespace Norvia.Application.Features.Eventos.ListarRiesgosDelEvento;

public interface IListarRiesgosDelEventoRepositorio
{
    Task<bool> ExisteAsync(Guid eventoId, CancellationToken ct);

    /// <summary>Los riesgos que reveló el evento y los que se reevaluaron a raíz de él, en ese orden.</summary>
    Task<List<RiesgoDelEventoResponseDto>> ListarAsync(Guid eventoId, CancellationToken ct);
}
