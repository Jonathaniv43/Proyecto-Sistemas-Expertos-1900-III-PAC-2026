using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.VincularMicrosoft;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class VincularMicrosoftRepositorio(NorviaDbContext db) : IVincularMicrosoftRepositorio
{
    public Task<bool> ProveedorVinculadoAsync(ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken) =>
        db.AccesoProveedor.AnyAsync(p => p.Proveedor == proveedor && p.ProveedorUserId == proveedorUserId, cancellationToken);

    public Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);

    public void Agregar(AccesoProveedor proveedor) => db.AccesoProveedor.Add(proveedor);
}
