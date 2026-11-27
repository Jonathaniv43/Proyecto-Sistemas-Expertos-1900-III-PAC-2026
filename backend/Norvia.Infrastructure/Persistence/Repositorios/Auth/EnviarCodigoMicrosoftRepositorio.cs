using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.EnviarCodigoMicrosoft;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Auth;

public sealed class EnviarCodigoMicrosoftRepositorio(NorviaDbContext db) : IEnviarCodigoMicrosoftRepositorio
{
    public Task<bool> ProveedorVinculadoAsync(ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken) =>
        db.AccesoProveedor.AnyAsync(p => p.Proveedor == proveedor && p.ProveedorUserId == proveedorUserId, cancellationToken);

    public Task<bool> ExisteEmailAsync(string emailNormalizado, CancellationToken cancellationToken) =>
        db.Acceso.AnyAsync(a => a.Email == emailNormalizado, cancellationToken);

    public Task<VerificacionCorreo?> BuscarAsync(string email, string proveedorUserId, CancellationToken cancellationToken) =>
        db.VerificacionCorreo.FirstOrDefaultAsync(
            v => v.Proposito == PropositoVerificacionCorreo.Microsoft && v.Email == email && v.ProveedorUserId == proveedorUserId,
            cancellationToken);

    public void Agregar(VerificacionCorreo fila) => db.VerificacionCorreo.Add(fila);
}
