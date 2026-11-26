using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.ConfirmarCambioContrasena;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class ConfirmarCambioContrasenaRepositorio(NorviaDbContext db) : IConfirmarCambioContrasenaRepositorio
{
    public Task<Acceso?> BuscarAccesoAsync(Guid accesoId, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Id == accesoId, cancellationToken);

    // Petición anónima: se borran las sesiones de todas las empresas, sin filtro de sesión.
    public async Task EliminarRefreshDelAccesoAsync(Guid accesoId, CancellationToken cancellationToken) =>
        db.RefreshToken.RemoveRange(
            await db.RefreshToken.IgnoreQueryFilters().Where(t => t.AccesoId == accesoId).ToListAsync(cancellationToken));
}
