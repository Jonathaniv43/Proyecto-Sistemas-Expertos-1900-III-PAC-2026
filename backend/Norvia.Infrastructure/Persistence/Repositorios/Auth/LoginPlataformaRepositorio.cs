using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.LoginPlataforma;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class LoginPlataformaRepositorio(NorviaDbContext db) : ILoginPlataformaRepositorio
{
    public Task<Acceso?> BuscarAccesoAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<StaffPlataforma?> BuscarStaffAsync(Guid accesoId, CancellationToken cancellationToken) =>
        db.StaffPlataforma
            .Include(s => s.RolStaffPlataforma)
            .FirstOrDefaultAsync(s => s.AccesoId == accesoId, cancellationToken);

    public void AgregarRefresh(RefreshToken refresh) => db.RefreshToken.Add(refresh);
}
