using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.RefrescarSesion;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class RefrescarSesionRepositorio(NorviaDbContext db) : IRefrescarSesionRepositorio
{
    public Task<RefreshToken?> BuscarRefreshAsync(string tokenHash, CancellationToken cancellationToken) =>
        db.RefreshToken
            .Include(t => t.Acceso).ThenInclude(a => a.Staff).ThenInclude(s => s!.RolStaffPlataforma)
            .Include(t => t.VinculoEmpresa).ThenInclude(v => v!.Rol)
            .Include(t => t.VinculoEmpresa).ThenInclude(v => v!.Empresa)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public void EliminarRefresh(RefreshToken refresh) => db.RefreshToken.Remove(refresh);

    public void AgregarRefresh(RefreshToken refresh) => db.RefreshToken.Add(refresh);
}
