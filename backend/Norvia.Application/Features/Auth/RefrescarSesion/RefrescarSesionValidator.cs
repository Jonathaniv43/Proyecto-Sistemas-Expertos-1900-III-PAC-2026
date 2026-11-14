using FluentValidation;

namespace Norvia.Application.Features.Auth.RefrescarSesion;

public sealed class RefrescarSesionValidator : AbstractValidator<RefrescarSesionRequest>
{
    public RefrescarSesionValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
