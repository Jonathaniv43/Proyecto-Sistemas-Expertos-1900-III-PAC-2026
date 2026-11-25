using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.LoginEmpresaMicrosoft;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class LoginEmpresaMicrosoftRepositorio(NorviaDbContext db) : ILoginEmpresaMicrosoftRepositorio
{
    public Task<Acceso?> BuscarAccesoPorProveedorAsync(
        ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken) =>
        db.AccesoProveedor
            .Where(p => p.Proveedor == proveedor && p.ProveedorUserId == proveedorUserId)
            .Select(p => p.Acceso)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<VinculoEmpresa?> BuscarVinculoAsync(Guid accesoId, Guid empresaId, CancellationToken cancellationToken) =>
        // IgnoreQueryFilters: en el login todavía no hay sesión; la empresa viene descifrada del request
        // y la consulta exige el AccesoId autenticado, así que no expone datos de otra empresa.
        db.VinculoEmpresa
            .IgnoreQueryFilters()
            .Include(v => v.Empresa)
            .Include(v => v.Rol)
            .FirstOrDefaultAsync(v => v.AccesoId == accesoId && v.EmpresaId == empresaId, cancellationToken);

    public Task<bool> UsuarioActivoAsync(Guid personaId, CancellationToken cancellationToken) =>
        db.Usuario.AnyAsync(u => u.PersonaId == personaId && u.Activo, cancellationToken);

    public void AgregarRefresh(RefreshToken refresh) => db.RefreshToken.Add(refresh);
}
