using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.PublicarVersionEncuesta;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class PublicarVersionEncuestaRepositorio(NorviaDbContext db) : IPublicarVersionEncuestaRepositorio
{
    public Task<Encuesta?> BuscarConVersionesAsync(Guid encuestaId, CancellationToken ct) =>
        db.Encuesta
            .Include(e => e.Versiones)
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.Opciones)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == encuestaId, ct);
}
