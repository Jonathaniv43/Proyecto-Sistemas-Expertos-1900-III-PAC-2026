using FluentValidation;

namespace Norvia.Application.Features.Auth.CerrarSesion;

public sealed class CerrarSesionValidator : AbstractValidator<CerrarSesionRequest>
{
    public CerrarSesionValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
