using FluentValidation;
using Norvia.Application.Features.Plan.GenerarPlan.DTOs;

namespace Norvia.Application.Features.Plan.GenerarPlan.Validators;

public sealed class GenerarPlanValidator : AbstractValidator<GenerarPlanRequestDto>
{
    public GenerarPlanValidator()
    {
        RuleFor(x => x.Claves).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Elija al menos una tarea para agregar al plan.")
            .Must(c => c!.Count <= 1000).WithMessage("Son demasiadas tareas para una sola vez.");
        RuleForEach(x => x.Claves).NotEmpty().MaximumLength(100);
    }
}
