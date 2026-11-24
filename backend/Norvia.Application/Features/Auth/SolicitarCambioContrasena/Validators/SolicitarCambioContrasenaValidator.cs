using FluentValidation;
using Norvia.Application.Features.Auth.SolicitarCambioContrasena.DTOs;

namespace Norvia.Application.Features.Auth.SolicitarCambioContrasena.Validators;

public sealed class SolicitarCambioContrasenaValidator : AbstractValidator<SolicitarCambioContrasenaRequestDto>
{
    public SolicitarCambioContrasenaValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
    }
}
