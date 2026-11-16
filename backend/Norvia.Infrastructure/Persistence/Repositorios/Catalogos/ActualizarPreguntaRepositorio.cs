using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ActualizarPregunta;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ActualizarPreguntaRepositorio(NorviaDbContext db) : IActualizarPreguntaRepositorio
{
    public Task<PreguntaExploratoria?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.PreguntaExploratoria
            .Include(p => p.Opciones)
            .Include(p => p.TipoRespuestaPregunta)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> CategoriaActivaExisteAsync(Guid categoriaRiesgoId, CancellationToken ct) =>
        db.CategoriaRiesgo.AnyAsync(c => c.Id == categoriaRiesgoId && c.Activa, ct);

    public Task<bool> PeligroActivoExisteAsync(Guid peligroId, CancellationToken ct) =>
        db.Peligro.AnyAsync(p => p.Id == peligroId && p.Activo, ct);

    public async Task<short> UltimoOrdenAsync(Guid categoriaRiesgoId, CancellationToken ct) =>
        await db.PreguntaExploratoria.Where(p => p.CategoriaRiesgoId == categoriaRiesgoId)
            .MaxAsync(p => (short?)p.Orden, ct) ?? 0;

    public Task<bool> OrdenOcupadoAsync(Guid categoriaRiesgoId, short orden, Guid excepto, CancellationToken ct) =>
        db.PreguntaExploratoria.AnyAsync(
            p => p.CategoriaRiesgoId == categoriaRiesgoId && p.Orden == orden && p.Id != excepto, ct);

    public Task<Guid?> BuscarTipoIdPorCodigoAsync(string codigo, CancellationToken ct) =>
        db.TipoRespuestaPregunta.AsNoTracking()
            .Where(t => t.Activo && t.Codigo == codigo)
            .Select(t => (Guid?)t.Id)
            .FirstOrDefaultAsync(ct);

    public void ReemplazarOpciones(PreguntaExploratoria pregunta, IEnumerable<OpcionPregunta> nuevas)
    {
        db.OpcionPregunta.RemoveRange(pregunta.Opciones);
        pregunta.Opciones.Clear();
        foreach (var opcion in nuevas)
        {
            pregunta.Opciones.Add(opcion);
            db.OpcionPregunta.Add(opcion);
        }
    }
}
