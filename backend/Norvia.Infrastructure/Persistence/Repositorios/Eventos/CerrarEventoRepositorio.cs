using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.CerrarEvento;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

public sealed class CerrarEventoRepositorio(NorviaDbContext db) : ICerrarEventoRepositorio
{
    public Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.Accidente.Include(a => a.Investigacion).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<int> MedidasAsync(Guid id, CancellationToken ct) =>
        db.Tarea.CountAsync(t => t.AccidenteId == id && t.Estado != EstadoTarea.Cancelada, ct);

    public async Task<EventoDetalleResponseDto> DetalleAsync(Guid id, CancellationToken ct) => (await ProyeccionEvento.DetalleAsync(db, id, ct))!;
}
