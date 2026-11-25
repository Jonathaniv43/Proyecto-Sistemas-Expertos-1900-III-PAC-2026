using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.IdentidadMicrosoft;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class IdentidadMicrosoftRepositorio(NorviaDbContext db) : IIdentidadMicrosoftRepositorio
{
    public Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.AsNoTracking().FirstOrDefaultAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<Acceso?> BuscarAccesoPorProveedorAsync(
        ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken) =>
        db.AccesoProveedor.AsNoTracking()
            .Where(p => p.Proveedor == proveedor && p.ProveedorUserId == proveedorUserId)
            .Select(p => p.Acceso)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Persona?> BuscarPersonaDeAccesoAsync(Guid accesoId, CancellationToken cancellationToken) =>
        // IgnoreQueryFilters: aún no hay sesión de empresa. La consulta exige el AccesoId ya validado con el
        // proveedor y solo devuelve la persona de esa cuenta, no datos de otra empresa.
        db.VinculoEmpresa.IgnoreQueryFilters().AsNoTracking()
            .Where(v => v.AccesoId == accesoId)
            .OrderBy(v => v.CreadoEn)
            .Select(v => v.Persona)
            .FirstOrDefaultAsync(cancellationToken);
}
