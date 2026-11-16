using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CambiarEstadoPeligro;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tabla global (sin EmpresaId): no le aplica el filtro por empresa.
public sealed class CambiarEstadoPeligroRepositorio(NorviaDbContext db) : ICambiarEstadoPeligroRepositorio
{
    public Task<Peligro?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.Peligro.FirstOrDefaultAsync(e => e.Id == id, ct);
}
