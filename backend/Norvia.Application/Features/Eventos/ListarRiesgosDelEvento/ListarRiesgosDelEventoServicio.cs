using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.ListarRiesgosDelEvento.DTOs;

namespace Norvia.Application.Features.Eventos.ListarRiesgosDelEvento;

/// <summary>
/// HU-051: lo que el evento cambió en la matriz (art. 47.b): los riesgos que reveló y los que se reevaluaron a raíz de
/// él, con el nivel de antes y el de después.
/// </summary>
public sealed class ListarRiesgosDelEventoServicio(IListarRiesgosDelEventoRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<RiesgoDelEventoResponseDto>>> EjecutarAsync(Guid eventoId, CancellationToken ct)
    {
        if (!await repositorio.ExisteAsync(eventoId, ct)) return Errores.EventoNoEncontrado;
        return await repositorio.ListarAsync(eventoId, ct);
    }
}
