using FluentValidation;
using Norvia.Application.Features.Auth.CerrarSesion.DTOs;

namespace Norvia.Application.Features.Auth.CerrarSesion.Validators;

/// <summary>
/// El refresh puede venir solo en cookie HttpOnly (body vacío). Logout es idempotente.
/// </summary>
public sealed class CerrarSesionValidator : AbstractValidator<CerrarSesionRequestDto>
{
    public CerrarSesionValidator()
    {
        RuleFor(x => x.RefreshToken).MaximumLength(512);
    }
}
