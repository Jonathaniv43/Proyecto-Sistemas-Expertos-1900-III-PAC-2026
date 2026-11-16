using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ObtenerEncuesta;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ObtenerEncuestaRepositorio(NorviaDbContext db) : IObtenerEncuestaRepositorio
{
    public Task<Encuesta?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.Encuesta.AsNoTracking()
            .Include(e => e.Versiones.Where(v =>
                v.Estado == EstadoVersionEncuesta.Publicada || v.Estado == EstadoVersionEncuesta.Borrador))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.Opciones)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
}
