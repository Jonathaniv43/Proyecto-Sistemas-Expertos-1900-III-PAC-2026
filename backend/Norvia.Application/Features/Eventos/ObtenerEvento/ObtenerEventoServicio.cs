using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.DTOs;

namespace Norvia.Application.Features.Eventos.ObtenerEvento;

/// <summary>HU-047, HU-049: detalle de un evento de la empresa, con su investigación, cierre o descarte. De otra empresa, 404.</summary>
public sealed class ObtenerEventoServicio(IObtenerEventoRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<EventoDetalleResponseDto>> EjecutarAsync(Guid id, CancellationToken ct) =>
        await repositorio.DetalleAsync(id, ct) is { } evento ? evento : Errores.EventoNoEncontrado;
}
