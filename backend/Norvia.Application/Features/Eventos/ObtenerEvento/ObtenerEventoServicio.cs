using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.ObtenerEvento.DTOs;

namespace Norvia.Application.Features.Eventos.ObtenerEvento;

public sealed class ObtenerEventoServicio(IObtenerEventoRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<EventoDetalleResponseDto>> EjecutarAsync(Guid id, CancellationToken ct)
    {
        var evento = await repositorio.BuscarAsync(id, ct);
        if (evento is null)
            return Errores.EventoNoEncontrado;

        return new EventoDetalleResponseDto(
            evento.Id,
            evento.Tipo,
            evento.Estado,
            evento.EstadoEnfermedad,
            evento.EsGrave,
            evento.FechaHoraEvento,
            evento.Lugar,
            evento.Descripcion,
            evento.RegistradoEn);
    }
}
