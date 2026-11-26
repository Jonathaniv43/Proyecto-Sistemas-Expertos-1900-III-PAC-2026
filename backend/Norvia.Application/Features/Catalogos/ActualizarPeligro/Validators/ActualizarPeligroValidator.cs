using FluentValidation;
using Norvia.Application.Features.Catalogos.ActualizarPeligro.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarPeligro.Validators;

public sealed class ActualizarPeligroValidator : AbstractValidator<ActualizarPeligroRequestDto>
{
    public ActualizarPeligroValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150).When(x => x.Nombre is not null);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
        RuleFor(x => x.Clasificacion).IsInEnum();
    }
}
