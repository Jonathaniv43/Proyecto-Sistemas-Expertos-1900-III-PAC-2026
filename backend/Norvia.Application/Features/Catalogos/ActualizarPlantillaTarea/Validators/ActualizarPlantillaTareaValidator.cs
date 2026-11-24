using FluentValidation;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea.Validators;

public sealed class ActualizarPlantillaTareaValidator : AbstractValidator<ActualizarPlantillaTareaRequestDto>
{
    public ActualizarPlantillaTareaValidator()
    {
        RuleFor(x => x.TipoControl).IsInEnum();
        RuleFor(x => x).Must(x => !(x.TipoControl is not null && x.QuitarTipoControl == true))
            .WithName("TipoControl").WithMessage("Indique un tipo de control o quítelo, no las dos cosas.");
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200).When(x => x.Titulo is not null);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
        RuleFor(x => x.PrioridadSugerida).InclusiveBetween((short)1, (short)3).When(x => x.PrioridadSugerida is not null)
            .WithMessage("La prioridad va de 1 (alta) a 3 (baja).");
    }
}
