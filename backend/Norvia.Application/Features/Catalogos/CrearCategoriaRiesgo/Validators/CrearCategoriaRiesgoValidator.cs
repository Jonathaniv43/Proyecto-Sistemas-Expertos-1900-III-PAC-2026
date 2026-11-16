using FluentValidation;
using Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo.DTOs;

namespace Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo.Validators;

public sealed class CrearCategoriaRiesgoValidator : AbstractValidator<CrearCategoriaRiesgoRequestDto>
{
    public CrearCategoriaRiesgoValidator()
    {
        RuleFor(x => x.RubroId).GreaterThan((short)0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
    }
}
