using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ListarPeligros;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ListarPeligrosRepositorio(NorviaDbContext db) : IListarPeligrosRepositorio
{
    public Task<List<Peligro>> ListarAsync(CancellationToken ct) =>
        db.Peligro.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(ct);
}
