using FluentValidation;
using Norvia.Application.Features.Auth.LoginPlataforma.DTOs;

namespace Norvia.Application.Features.Auth.LoginPlataforma.Validators;

public sealed class LoginPlataformaValidator : AbstractValidator<LoginPlataformaRequestDto>
{
    public LoginPlataformaValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
