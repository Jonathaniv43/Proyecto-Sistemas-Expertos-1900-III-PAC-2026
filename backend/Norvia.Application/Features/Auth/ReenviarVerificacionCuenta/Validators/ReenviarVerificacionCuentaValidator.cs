using FluentValidation;
using Norvia.Application.Features.Auth.ReenviarVerificacionCuenta.DTOs;

namespace Norvia.Application.Features.Auth.ReenviarVerificacionCuenta.Validators;

public sealed class ReenviarVerificacionCuentaValidator : AbstractValidator<ReenviarVerificacionCuentaRequestDto>
{
    public ReenviarVerificacionCuentaValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
    }
}
