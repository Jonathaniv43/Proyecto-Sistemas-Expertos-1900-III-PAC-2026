using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.ObtenerEvento;
using Norvia.Domain.Entities.Accidentes;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

public sealed class ObtenerEventoRepositorio(NorviaDbContext db) : IObtenerEventoRepositorio
{
    public Task<Accidente?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.Accidente.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
}
