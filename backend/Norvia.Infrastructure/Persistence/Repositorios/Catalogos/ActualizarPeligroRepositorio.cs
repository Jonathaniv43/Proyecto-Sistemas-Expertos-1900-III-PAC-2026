using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ActualizarPeligro;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ActualizarPeligroRepositorio(NorviaDbContext db) : IActualizarPeligroRepositorio
{
    public Task<Peligro?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.Peligro.FirstOrDefaultAsync(p => p.Id == id, ct);
}
