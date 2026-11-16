using FluentValidation;
using Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo.Validators;

public sealed class ActualizarCategoriaRiesgoValidator : AbstractValidator<ActualizarCategoriaRiesgoRequestDto>
{
    public ActualizarCategoriaRiesgoValidator()
    {
        RuleFor(x => x.RubroId).GreaterThan((short)0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100).When(x => x.Nombre is not null);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
    }
}
