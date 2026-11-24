using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.DescartarEvento;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

public sealed class DescartarEventoRepositorio(NorviaDbContext db) : IDescartarEventoRepositorio
{
    public Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct) => db.Accidente.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<EventoDetalleResponseDto> DetalleAsync(Guid id, CancellationToken ct) => (await ProyeccionEvento.DetalleAsync(db, id, ct))!;
}
