using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ListarPreguntas;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ListarPreguntasRepositorio(NorviaDbContext db) : IListarPreguntasRepositorio
{
    public Task<List<PreguntaExploratoria>> ListarAsync(Guid? categoriaRiesgoId, CancellationToken ct) =>
        db.PreguntaExploratoria.AsNoTracking()
            .Include(p => p.Opciones)
            .Where(p => categoriaRiesgoId == null || p.CategoriaRiesgoId == categoriaRiesgoId)
            .OrderBy(p => p.CategoriaRiesgoId).ThenBy(p => p.Orden)
            .AsSplitQuery()
            .ToListAsync(ct);
}
