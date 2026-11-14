using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.LoginEmpresa;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class LoginEmpresaRepositorio(NorviaDbContext db) : ILoginEmpresaRepositorio
{
    public Task<Acceso?> BuscarAccesoAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<VinculoEmpresa?> BuscarVinculoAsync(Guid accesoId, Guid empresaId, CancellationToken cancellationToken) =>
        // IgnoreQueryFilters: en el login todavía no hay sesión; la empresa viene descifrada del request
        // y la consulta exige el AccesoId autenticado, así que no expone datos de otra empresa.
        db.VinculoEmpresa
            .IgnoreQueryFilters()
            .Include(v => v.Empresa)
            .Include(v => v.Rol)
            .FirstOrDefaultAsync(v => v.AccesoId == accesoId && v.EmpresaId == empresaId, cancellationToken);

    public void AgregarRefresh(RefreshToken refresh) => db.RefreshToken.Add(refresh);
}
