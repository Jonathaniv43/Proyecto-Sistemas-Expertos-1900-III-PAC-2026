using FluentValidation;
using Norvia.Application.Features.Encuestas.CrearEncuesta.DTOs;

namespace Norvia.Application.Features.Encuestas.CrearEncuesta.Validators;

public sealed class CrearEncuestaValidator : AbstractValidator<CrearEncuestaRequestDto>
{
    public CrearEncuestaValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CategoriaRiesgoIds)
            .NotNull()
            .Must(ids => ids is not null && ids.Count > 0)
            .WithMessage("Debe elegir al menos una categoría de riesgo.")
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count)
            .WithMessage("No envíe categorías de riesgo repetidas.");

        RuleForEach(x => x.CategoriaRiesgoIds).NotEmpty();
    }
}
