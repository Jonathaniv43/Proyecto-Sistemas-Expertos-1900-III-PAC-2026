using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.EstablecerContrasena;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

// Acceso y RefreshToken son tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class EstablecerContrasenaRepositorio(NorviaDbContext db) : IEstablecerContrasenaRepositorio
{
    public Task<Acceso?> BuscarAccesoAsync(Guid accesoId, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Id == accesoId, cancellationToken);

    // Se cargan y se marcan para borrar (sin ExecuteDelete) para que todo se confirme en una sola transacción.
    public async Task EliminarRefreshDelAccesoAsync(Guid accesoId, CancellationToken cancellationToken) =>
        db.RefreshToken.RemoveRange(
            await db.RefreshToken.Where(t => t.AccesoId == accesoId).ToListAsync(cancellationToken));
}
