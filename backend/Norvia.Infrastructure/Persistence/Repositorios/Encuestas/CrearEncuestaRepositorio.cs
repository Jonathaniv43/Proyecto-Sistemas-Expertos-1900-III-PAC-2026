using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.CrearEncuesta;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class CrearEncuestaRepositorio(NorviaDbContext db) : ICrearEncuestaRepositorio
{
    public Task<short?> BuscarRubroDeEmpresaAsync(Guid empresaId, CancellationToken ct) =>
        db.Empresa.AsNoTracking().Where(e => e.Id == empresaId).Select(e => e.RubroId).FirstOrDefaultAsync(ct);

    public Task<List<CategoriaRiesgo>> ListarCategoriasActivasConPreguntasAsync(
        short rubroId, IReadOnlyList<Guid> categoriaIds, CancellationToken ct) =>
        db.CategoriaRiesgo.AsNoTracking()
            .Where(c => c.RubroId == rubroId && c.Activa && categoriaIds.Contains(c.Id))
            .Include(c => c.Preguntas.Where(p => p.Activa))
                .ThenInclude(p => p.Opciones)
            .Include(c => c.Preguntas.Where(p => p.Activa))
                .ThenInclude(p => p.TipoRespuestaPregunta)
            .AsSplitQuery()
            .ToListAsync(ct);

    public void Agregar(Encuesta encuesta) => db.Encuesta.Add(encuesta);
}
