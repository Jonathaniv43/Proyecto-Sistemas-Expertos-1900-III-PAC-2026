using FluentValidation;
using Norvia.Application.Features.Auth.LoginEmpresa.DTOs;

namespace Norvia.Application.Features.Auth.LoginEmpresa.Validators;

public sealed class LoginEmpresaValidator : AbstractValidator<LoginEmpresaRequestDto>
{
    public LoginEmpresaValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.IdEmpresa).NotEmpty();
    }
}
