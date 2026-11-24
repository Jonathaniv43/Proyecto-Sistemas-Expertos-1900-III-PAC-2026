using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Application.Features.Eventos.ObtenerEvento;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

public sealed class ObtenerEventoRepositorio(NorviaDbContext db) : IObtenerEventoRepositorio
{
    public Task<EventoDetalleResponseDto?> DetalleAsync(Guid id, CancellationToken ct) => ProyeccionEvento.DetalleAsync(db, id, ct);
}
