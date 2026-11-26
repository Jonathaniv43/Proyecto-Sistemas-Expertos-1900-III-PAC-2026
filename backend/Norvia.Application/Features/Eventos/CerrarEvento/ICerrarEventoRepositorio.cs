using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Accidentes;

namespace Norvia.Application.Features.Eventos.CerrarEvento;

public interface ICerrarEventoRepositorio
{
    /// <summary>El evento de la empresa de la sesión con su investigación (con seguimiento); null si no existe.</summary>
    Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct);

    /// <summary>Medidas correctivas del evento: tareas con su AccidenteId que no están canceladas (HU-050).</summary>
    Task<int> MedidasAsync(Guid id, CancellationToken ct);

    Task<EventoDetalleResponseDto> DetalleAsync(Guid id, CancellationToken ct);
}
