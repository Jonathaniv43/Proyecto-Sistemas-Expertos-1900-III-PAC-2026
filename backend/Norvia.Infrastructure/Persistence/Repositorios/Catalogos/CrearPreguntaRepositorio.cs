using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CrearPregunta;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class CrearPreguntaRepositorio(NorviaDbContext db) : ICrearPreguntaRepositorio
{
    public Task<bool> CategoriaActivaExisteAsync(Guid categoriaRiesgoId, CancellationToken ct) =>
        db.CategoriaRiesgo.AnyAsync(c => c.Id == categoriaRiesgoId && c.Activa, ct);

    public Task<bool> PeligroActivoExisteAsync(Guid peligroId, CancellationToken ct) =>
        db.Peligro.AnyAsync(p => p.Id == peligroId && p.Activo, ct);

    public async Task<short> UltimoOrdenAsync(Guid categoriaRiesgoId, CancellationToken ct) =>
        await db.PreguntaExploratoria.Where(p => p.CategoriaRiesgoId == categoriaRiesgoId)
            .MaxAsync(p => (short?)p.Orden, ct) ?? 0;

    public void Agregar(PreguntaExploratoria pregunta) => db.PreguntaExploratoria.Add(pregunta);
}
