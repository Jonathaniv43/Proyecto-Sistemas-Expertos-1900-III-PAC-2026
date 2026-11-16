using FluentValidation;
using Norvia.Application.Common.Validacion;
using Norvia.Application.Features.Auth.VerificarCuenta.DTOs;

namespace Norvia.Application.Features.Auth.VerificarCuenta.Validators;

public sealed class VerificarCuentaValidator : AbstractValidator<VerificarCuentaRequestDto>
{
    public VerificarCuentaValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.ContrasenaTemporal).NotEmpty().WithMessage("Ingrese la contraseña temporal.");
        RuleFor(x => x.ContrasenaNueva)
            .CumplePoliticaContrasena()
            .NotEqual(x => x.ContrasenaTemporal)
            .WithMessage("La contraseña nueva debe ser distinta de la temporal.");
        RuleFor(x => x.ContrasenaNuevaRepetida)
            .Equal(x => x.ContrasenaNueva)
            .WithMessage("La confirmación de la contraseña no coincide.");
    }
}
