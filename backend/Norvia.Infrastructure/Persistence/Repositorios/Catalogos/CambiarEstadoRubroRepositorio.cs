using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CambiarEstadoRubro;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tabla global (sin EmpresaId): no le aplica el filtro por empresa.
public sealed class CambiarEstadoRubroRepositorio(NorviaDbContext db) : ICambiarEstadoRubroRepositorio
{
    public Task<Rubro?> BuscarAsync(short id, CancellationToken ct) =>
        db.Rubro.FirstOrDefaultAsync(e => e.Id == id, ct);
}
