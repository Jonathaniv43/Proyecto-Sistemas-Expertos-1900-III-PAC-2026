using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.CerrarSesion;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class CerrarSesionRepositorio(NorviaDbContext db) : ICerrarSesionRepositorio
{
    public Task<RefreshToken?> BuscarRefreshAsync(string tokenHash, CancellationToken cancellationToken) =>
        db.RefreshToken.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public void EliminarRefresh(RefreshToken refresh) => db.RefreshToken.Remove(refresh);
}
