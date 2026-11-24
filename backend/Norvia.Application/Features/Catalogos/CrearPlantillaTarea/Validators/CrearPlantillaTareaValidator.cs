using FluentValidation;
using Norvia.Application.Features.Catalogos.CrearPlantillaTarea.DTOs;

namespace Norvia.Application.Features.Catalogos.CrearPlantillaTarea.Validators;

public sealed class CrearPlantillaTareaValidator : AbstractValidator<CrearPlantillaTareaRequestDto>
{
    public CrearPlantillaTareaValidator()
    {
        RuleFor(x => x.PeligroId).NotNull().WithMessage("Indique el peligro.").NotEqual(Guid.Empty);
        RuleFor(x => x.TipoControl).IsInEnum();
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
        RuleFor(x => x.PrioridadSugerida).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Indique la prioridad sugerida.")
            .InclusiveBetween((short)1, (short)3).WithMessage("La prioridad va de 1 (alta) a 3 (baja).");
    }
}
