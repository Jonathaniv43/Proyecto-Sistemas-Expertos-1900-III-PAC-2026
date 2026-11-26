using FluentValidation;
using Norvia.Application.Features.Plan.AsignarTarea.DTOs;

namespace Norvia.Application.Features.Plan.AsignarTarea.Validators;

public sealed class AsignarTareaValidator : AbstractValidator<AsignarTareaRequestDto>
{
    public AsignarTareaValidator()
    {
        RuleFor(x => x.ResponsableId).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Elija a la persona responsable.")
            .NotEqual(Guid.Empty).WithMessage("Elija a la persona responsable.");
        RuleFor(x => x.FechaLimite).NotNull().WithMessage("Indique la fecha límite.");
    }
}
