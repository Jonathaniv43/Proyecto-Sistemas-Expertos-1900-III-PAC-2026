using FluentValidation;
using Norvia.Application.Features.Auth.CerrarSesion.DTOs;

namespace Norvia.Application.Features.Auth.CerrarSesion.Validators;

public sealed class CerrarSesionValidator : AbstractValidator<CerrarSesionRequestDto>
{
    public CerrarSesionValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
