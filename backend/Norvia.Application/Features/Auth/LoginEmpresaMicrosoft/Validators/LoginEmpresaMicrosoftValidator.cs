using FluentValidation;
using Norvia.Application.Features.Auth.LoginEmpresaMicrosoft.DTOs;

namespace Norvia.Application.Features.Auth.LoginEmpresaMicrosoft.Validators;

public sealed class LoginEmpresaMicrosoftValidator : AbstractValidator<LoginEmpresaMicrosoftRequestDto>
{
    public LoginEmpresaMicrosoftValidator()
    {
        RuleFor(x => x.TokenIdentidad).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.IdEmpresa).NotEmpty();
    }
}
