using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ListarRubros;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ListarRubrosRepositorio(NorviaDbContext db) : IListarRubrosRepositorio
{
    public Task<List<Rubro>> ListarAsync(CancellationToken ct) =>
        db.Rubro.AsNoTracking().OrderBy(r => r.Id).ToListAsync(ct);
}
