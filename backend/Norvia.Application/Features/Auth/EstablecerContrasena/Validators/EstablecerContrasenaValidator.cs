using FluentValidation;
using Norvia.Application.Common.Validacion;
using Norvia.Application.Features.Auth.EstablecerContrasena.DTOs;

namespace Norvia.Application.Features.Auth.EstablecerContrasena.Validators;

public sealed class EstablecerContrasenaValidator : AbstractValidator<EstablecerContrasenaRequestDto>
{
    public EstablecerContrasenaValidator()
    {
        RuleFor(x => x.ContrasenaActual).NotEmpty().WithMessage("Ingrese su contraseña actual.");
        RuleFor(x => x.ContrasenaNueva)
            .CumplePoliticaContrasena()
            .NotEqual(x => x.ContrasenaActual).WithMessage("La contraseña nueva debe ser distinta de la actual.");
    }
}
