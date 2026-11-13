using Microsoft.AspNetCore.Identity;
using Norvia.Application.Features.Auth.Interfaces;
using Norvia.Domain.Entities.Acceso;

namespace Norvia.Infrastructure.Auth;

public class VerificadorPassword : IVerificadorPassword
{
    private readonly PasswordHasher<Acceso> _hasher = new();

    public bool Verificar(Acceso acceso, string password) =>
        _hasher.VerifyHashedPassword(acceso, acceso.PasswordHash, password) != PasswordVerificationResult.Failed;
}
