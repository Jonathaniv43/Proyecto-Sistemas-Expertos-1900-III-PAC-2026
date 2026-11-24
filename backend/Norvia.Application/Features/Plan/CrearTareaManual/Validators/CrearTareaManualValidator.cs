using FluentValidation;
using Norvia.Application.Features.Plan.CrearTareaManual.DTOs;

namespace Norvia.Application.Features.Plan.CrearTareaManual.Validators;

public sealed class CrearTareaManualValidator : AbstractValidator<CrearTareaManualRequestDto>
{
    public CrearTareaManualValidator()
    {
        RuleFor(x => x.Titulo).Cascade(CascadeMode.Stop)
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Indique qué hay que hacer.")
            .MaximumLength(200).WithMessage("El título no puede exceder 200 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(2000).WithMessage("La descripción no puede exceder 2000 caracteres.");
        RuleFor(x => x.Prioridad).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Elija la prioridad.")
            .InclusiveBetween((short)1, (short)3).WithMessage("La prioridad va de 1 (alta) a 3 (baja).");
        RuleFor(x => x.ResponsableId).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Elija a la persona responsable.")
            .NotEqual(Guid.Empty).WithMessage("Elija a la persona responsable.");
        RuleFor(x => x.FechaLimite).NotNull().WithMessage("Indique la fecha límite.");
    }
}
