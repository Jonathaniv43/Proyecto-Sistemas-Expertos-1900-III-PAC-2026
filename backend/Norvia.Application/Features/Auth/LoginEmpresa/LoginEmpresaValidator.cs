using FluentValidation;

namespace Norvia.Application.Features.Auth.LoginEmpresa;

public sealed class LoginEmpresaValidator : AbstractValidator<LoginEmpresaRequest>
{
    public LoginEmpresaValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.IdEmpresa).NotEmpty();
    }
}
