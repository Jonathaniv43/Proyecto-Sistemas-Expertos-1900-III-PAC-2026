using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ListarCategoriasRiesgo;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ListarCategoriasRiesgoRepositorio(NorviaDbContext db) : IListarCategoriasRiesgoRepositorio
{
    public Task<List<CategoriaRiesgo>> ListarAsync(short? rubroId, CancellationToken ct) =>
        db.CategoriaRiesgo.AsNoTracking()
            .Where(c => rubroId == null || c.RubroId == rubroId)
            .OrderBy(c => c.RubroId).ThenBy(c => c.Nombre)
            .ToListAsync(ct);
}
