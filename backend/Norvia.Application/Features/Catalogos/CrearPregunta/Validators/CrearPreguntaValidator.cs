using FluentValidation;
using Norvia.Application.Features.Catalogos.CrearPregunta.DTOs;

namespace Norvia.Application.Features.Catalogos.CrearPregunta.Validators;

public sealed class CrearPreguntaValidator : AbstractValidator<CrearPreguntaRequestDto>
{
    public CrearPreguntaValidator()
    {
        RuleFor(x => x.CategoriaRiesgoId).NotEmpty();
        RuleFor(x => x.Texto).NotEmpty().MaximumLength(500);
        Include(new ReglasPregunta());
    }
}
