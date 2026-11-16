using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.CerrarEncuesta;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class CerrarEncuestaRepositorio(NorviaDbContext db) : ICerrarEncuestaRepositorio
{
    public Task<Encuesta?> BuscarAsync(Guid encuestaId, CancellationToken ct) =>
        db.Encuesta.FirstOrDefaultAsync(e => e.Id == encuestaId, ct);
}
