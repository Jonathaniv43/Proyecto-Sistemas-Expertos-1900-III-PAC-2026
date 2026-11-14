using FluentValidation;

namespace Norvia.Application.Features.Auth.LoginPlataforma;

public sealed class LoginPlataformaValidator : AbstractValidator<LoginPlataformaRequest>
{
    public LoginPlataformaValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
