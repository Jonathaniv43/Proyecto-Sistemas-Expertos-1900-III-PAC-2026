using Norvia.Application.Features.Eventos.DTOs;

namespace Norvia.Application.Features.Eventos.ObtenerEvento;

public interface IObtenerEventoRepositorio
{
    /// <summary>El evento de la empresa de la sesión con su investigación y sus medidas; null si no existe.</summary>
    Task<EventoDetalleResponseDto?> DetalleAsync(Guid id, CancellationToken ct);
}
