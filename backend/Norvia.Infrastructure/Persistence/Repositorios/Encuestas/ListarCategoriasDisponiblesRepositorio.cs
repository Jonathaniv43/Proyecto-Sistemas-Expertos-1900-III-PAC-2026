using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ListarCategoriasDisponibles;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ListarCategoriasDisponiblesRepositorio(NorviaDbContext db) : IListarCategoriasDisponiblesRepositorio
{
    // Empresa es la tabla raíz (sin filtro global): se busca por el Id que viene del token.
    public Task<short?> BuscarRubroDeEmpresaAsync(Guid empresaId, CancellationToken ct) =>
        db.Empresa.AsNoTracking().Where(e => e.Id == empresaId).Select(e => e.RubroId).FirstOrDefaultAsync(ct);

    // Catálogo global (sin EmpresaId). Solo lo activo: lo desactivado no se ofrece en encuestas nuevas.
    public Task<List<CategoriaRiesgo>> ListarActivasDelRubroAsync(short rubroId, CancellationToken ct) =>
        db.CategoriaRiesgo.AsNoTracking()
            .Where(c => c.RubroId == rubroId && c.Activa)
            .Include(c => c.Preguntas.Where(p => p.Activa))
                .ThenInclude(p => p.Opciones)
            .Include(c => c.Preguntas.Where(p => p.Activa))
                .ThenInclude(p => p.TipoRespuestaPregunta)
            .OrderBy(c => c.Nombre)
            .AsSplitQuery()
            .ToListAsync(ct);
}
