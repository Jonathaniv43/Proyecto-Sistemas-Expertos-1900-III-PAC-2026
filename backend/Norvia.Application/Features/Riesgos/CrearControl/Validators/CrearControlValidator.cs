using FluentValidation;
using Norvia.Application.Features.Riesgos.CrearControl.DTOs;

namespace Norvia.Application.Features.Riesgos.CrearControl.Validators;

public sealed class CrearControlValidator : AbstractValidator<CrearControlRequestDto>
{
    public CrearControlValidator()
    {
        RuleFor(x => x.Tipo).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Indique el tipo de control.")
            .IsInEnum().WithMessage("El tipo de control no es válido.");
        RuleFor(x => x.Descripcion).Cascade(CascadeMode.Stop)
            .Must(d => !string.IsNullOrWhiteSpace(d)).WithMessage("Describa el control.")
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.");
        RuleFor(x => x.PlantillaId).NotEqual(Guid.Empty).When(x => x.PlantillaId is not null);
    }
}
