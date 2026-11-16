using FluentValidation;
using Norvia.Application.Features.Catalogos.ActualizarRubro.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarRubro.Validators;

public sealed class ActualizarRubroValidator : AbstractValidator<ActualizarRubroRequestDto>
{
    public ActualizarRubroValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(80).When(x => x.Nombre is not null);
    }
}
