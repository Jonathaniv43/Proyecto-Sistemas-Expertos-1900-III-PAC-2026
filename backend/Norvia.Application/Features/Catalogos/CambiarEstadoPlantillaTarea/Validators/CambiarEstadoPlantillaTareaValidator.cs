using FluentValidation;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaTarea.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaTarea.Validators;

public sealed class CambiarEstadoPlantillaTareaValidator : AbstractValidator<CambiarEstadoPlantillaTareaRequestDto>
{
    public CambiarEstadoPlantillaTareaValidator()
    {
        RuleFor(x => x.Activo).NotNull();
    }
}
