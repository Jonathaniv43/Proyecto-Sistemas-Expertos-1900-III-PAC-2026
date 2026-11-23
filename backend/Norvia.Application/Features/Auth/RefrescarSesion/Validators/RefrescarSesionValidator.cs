using FluentValidation;
using Norvia.Application.Features.Auth.RefrescarSesion.DTOs;

namespace Norvia.Application.Features.Auth.RefrescarSesion.Validators;

/// <summary>
/// El refresh puede venir solo en cookie HttpOnly (body vacío).
/// Si falta en ambos, el servicio responde sesión inválida (401).
/// </summary>
public sealed class RefrescarSesionValidator : AbstractValidator<RefrescarSesionRequestDto>
{
    public RefrescarSesionValidator()
    {
        RuleFor(x => x.RefreshToken).MaximumLength(512);
    }
}
