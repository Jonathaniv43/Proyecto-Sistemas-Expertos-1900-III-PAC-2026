using FluentValidation;
using Norvia.Application.Common.Validacion;
using Norvia.Application.Features.Auth.VerificarCuenta.DTOs;

namespace Norvia.Application.Features.Auth.VerificarCuenta.Validators;

public sealed class VerificarCuentaValidator : AbstractValidator<VerificarCuentaRequestDto>
{
    public VerificarCuentaValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Codigo).NotEmpty().Length(6).WithMessage("Ingrese el código de 6 dígitos que le enviamos.");
        RuleFor(x => x.ContrasenaNueva).CumplePoliticaContrasena();
        RuleFor(x => x.ContrasenaNuevaRepetida)
            .Equal(x => x.ContrasenaNueva)
            .WithMessage("La confirmación de la contraseña no coincide.");
    }
}
