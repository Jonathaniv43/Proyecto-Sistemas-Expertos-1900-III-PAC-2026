using FluentValidation;
using Norvia.Application.Features.Auth.RefrescarSesion.DTOs;

namespace Norvia.Application.Features.Auth.RefrescarSesion.Validators;

public sealed class RefrescarSesionValidator : AbstractValidator<RefrescarSesionRequestDto>
{
    public RefrescarSesionValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
