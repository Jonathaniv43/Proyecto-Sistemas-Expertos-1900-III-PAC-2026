using FluentValidation;
using Norvia.Application.Features.Riesgos.CrearRiesgo.DTOs;

namespace Norvia.Application.Features.Riesgos.CrearRiesgo.Validators;

public sealed class CrearRiesgoValidator : AbstractValidator<CrearRiesgoRequestDto>
{
    public CrearRiesgoValidator()
    {
        RuleFor(x => x.PeligroId).NotEqual(Guid.Empty).When(x => x.PeligroId is not null);
        RuleFor(x => x.AccidenteId).NotEqual(Guid.Empty).When(x => x.AccidenteId is not null);
        RuleFor(x => x.DescripcionPeligro)
            .MaximumLength(300).WithMessage("La descripción del peligro no puede exceder 300 caracteres.");
        RuleFor(x => x)
            .Must(x => (x.PeligroId is not null) != !string.IsNullOrWhiteSpace(x.DescripcionPeligro))
            .WithName("Solicitud")
            .WithMessage("Elija un peligro del catálogo o descríbalo, pero no ambos.");
        RuleFor(x => x.Actividad)
            .NotEmpty().WithMessage("Indique la actividad relacionada.")
            .MaximumLength(200).WithMessage("La actividad no puede exceder 200 caracteres.");
    }
}
