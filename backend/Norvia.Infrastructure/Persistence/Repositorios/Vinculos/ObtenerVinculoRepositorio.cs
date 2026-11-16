using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Vinculos.ObtenerVinculo;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Vinculos;

// El filtro global limita a la empresa de la sesión: un id de otra empresa no se encuentra.
public sealed class ObtenerVinculoRepositorio(NorviaDbContext db) : IObtenerVinculoRepositorio
{
    public Task<VinculoEmpresa?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.VinculoEmpresa.AsNoTracking()
            .Include(v => v.Persona).Include(v => v.Acceso).Include(v => v.Rol)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
}
