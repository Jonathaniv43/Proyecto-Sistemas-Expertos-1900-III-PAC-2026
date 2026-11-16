using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CambiarEstadoPregunta;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tabla global (sin EmpresaId): no le aplica el filtro por empresa.
public sealed class CambiarEstadoPreguntaRepositorio(NorviaDbContext db) : ICambiarEstadoPreguntaRepositorio
{
    public Task<PreguntaExploratoria?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.PreguntaExploratoria
            .Include(p => p.Opciones)
            .Include(p => p.TipoRespuestaPregunta)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
}
