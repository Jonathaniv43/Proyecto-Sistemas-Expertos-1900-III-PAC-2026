using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ListarTiposRespuesta;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ListarTiposRespuestaRepositorio(NorviaDbContext db) : IListarTiposRespuestaRepositorio
{
    public Task<List<TipoRespuestaPreguntaCatalogo>> ListarActivosAsync(CancellationToken ct) =>
        db.TipoRespuestaPregunta.AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Codigo)
            .ToListAsync(ct);
}
