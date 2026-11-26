using FluentValidation;
using Norvia.Application.Common.Validacion;
using Norvia.Application.Features.Auth.ReestablecerContrasena.DTOs;

namespace Norvia.Application.Features.Auth.ReestablecerContrasena.Validators;

public sealed class ReestablecerContrasenaValidator : AbstractValidator<ReestablecerContrasenaRequestDto>
{
    public ReestablecerContrasenaValidator()
    {
        RuleFor(x => x.ContrasenaActual).NotEmpty().WithMessage("Ingrese su contraseña actual.");
        RuleFor(x => x.ContrasenaNueva)
            .CumplePoliticaContrasena()
            .NotEqual(x => x.ContrasenaActual).WithMessage("La contraseña nueva debe ser distinta de la actual.");
    }
}