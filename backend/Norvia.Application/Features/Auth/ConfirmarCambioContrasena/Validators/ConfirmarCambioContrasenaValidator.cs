using FluentValidation;
using Norvia.Application.Common.Validacion;
using Norvia.Application.Features.Auth.ConfirmarCambioContrasena.DTOs;

namespace Norvia.Application.Features.Auth.ConfirmarCambioContrasena.Validators;

public sealed class ConfirmarCambioContrasenaValidator : AbstractValidator<ConfirmarCambioContrasenaRequestDto>
{
    public ConfirmarCambioContrasenaValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContrasenaNueva).CumplePoliticaContrasena();
    }
}
