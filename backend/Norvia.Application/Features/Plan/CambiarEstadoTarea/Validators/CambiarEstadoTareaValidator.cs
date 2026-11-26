using FluentValidation;
using Norvia.Application.Features.Plan.CambiarEstadoTarea.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.CambiarEstadoTarea.Validators;

public sealed class CambiarEstadoTareaValidator : AbstractValidator<CambiarEstadoTareaRequestDto>
{
    public CambiarEstadoTareaValidator()
    {
        // Cancelar no es parte de atender una tarea: lo decide la gestión del plan.
        RuleFor(x => x.Estado).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Indique el estado.")
            .Must(e => e is EstadoTarea.Pendiente or EstadoTarea.EnProgreso or EstadoTarea.Completada)
            .WithMessage("El estado puede ser pendiente, en progreso o completada.");
    }
}
