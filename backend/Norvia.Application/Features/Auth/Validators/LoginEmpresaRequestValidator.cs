using FluentValidation;
using Norvia.Application.Features.Auth.DTOs;

namespace Norvia.Application.Features.Auth.Validators;

public class LoginEmpresaRequestValidator : AbstractValidator<LoginEmpresaRequest>
{
    public LoginEmpresaRequestValidator()
    {
        Include(new LoginRequestValidator());
        RuleFor(x => x.IdEmpresa).NotEmpty();
    }
}
