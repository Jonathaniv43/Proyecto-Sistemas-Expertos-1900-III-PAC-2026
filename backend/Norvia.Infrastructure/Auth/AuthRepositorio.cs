using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.Interfaces;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Persistence;

namespace Norvia.Infrastructure.Auth;

public class AuthRepositorio(NorviaDbContext db) : IAuthRepositorio
{
    public Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<VinculoEmpresa?> BuscarVinculoConEmpresaYRolAsync(
        Guid accesoId,
        Guid empresaId,
        CancellationToken cancellationToken = default) =>
        db.VinculoEmpresa
            .Include(v => v.Empresa)
            .Include(v => v.Rol)
            .FirstOrDefaultAsync(v => v.AccesoId == accesoId && v.EmpresaId == empresaId, cancellationToken);

    public Task<StaffPlataforma?> BuscarStaffConRolAsync(Guid accesoId, CancellationToken cancellationToken = default) =>
        db.StaffPlataforma
            .Include(s => s.RolStaffPlataforma)
            .FirstOrDefaultAsync(s => s.AccesoId == accesoId, cancellationToken);

    public Task<RefreshToken?> BuscarRefreshConSesionAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        db.RefreshToken
            .Include(t => t.Acceso).ThenInclude(a => a.Staff).ThenInclude(s => s!.RolStaffPlataforma)
            .Include(t => t.VinculoEmpresa).ThenInclude(v => v!.Rol)
            .Include(t => t.VinculoEmpresa).ThenInclude(v => v!.Empresa)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task EliminarRefreshAsync(RefreshToken refresh, CancellationToken cancellationToken = default)
    {
        db.RefreshToken.Remove(refresh);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AgregarRefreshAsync(RefreshToken refresh, CancellationToken cancellationToken = default)
    {
        db.RefreshToken.Add(refresh);
        await db.SaveChangesAsync(cancellationToken);
    }
}
