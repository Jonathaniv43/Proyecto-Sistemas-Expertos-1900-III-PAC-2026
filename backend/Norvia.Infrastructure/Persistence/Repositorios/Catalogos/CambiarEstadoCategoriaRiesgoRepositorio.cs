using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tabla global (sin EmpresaId): no le aplica el filtro por empresa.
public sealed class CambiarEstadoCategoriaRiesgoRepositorio(NorviaDbContext db) : ICambiarEstadoCategoriaRiesgoRepositorio
{
    public Task<CategoriaRiesgo?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.CategoriaRiesgo.FirstOrDefaultAsync(e => e.Id == id, ct);
}
