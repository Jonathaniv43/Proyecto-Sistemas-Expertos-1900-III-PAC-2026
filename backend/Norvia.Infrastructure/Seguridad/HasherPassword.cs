using Microsoft.AspNetCore.Identity;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Seguridad;

/// <summary>Hash de contraseñas con <see cref="PasswordHasher{TUser}"/>. Lo usan el login y los seeders.</summary>
public sealed class HasherPassword : IHasherPassword
{
    private readonly PasswordHasher<Acceso> _hasher = new();

    public string Hashear(Acceso acceso, string password) => _hasher.HashPassword(acceso, password);

    /// <summary>Una cuenta sin contraseña (solo proveedor, D29) nunca verifica.</summary>
    public bool Verificar(Acceso acceso, string password) =>
        acceso.PasswordHash is not null
        && _hasher.VerifyHashedPassword(acceso, acceso.PasswordHash, password) != PasswordVerificationResult.Failed;
}
