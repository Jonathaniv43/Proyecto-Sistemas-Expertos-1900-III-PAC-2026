using FluentValidation;
using Norvia.Application.Features.Catalogos.CrearPeligro.DTOs;

namespace Norvia.Application.Features.Catalogos.CrearPeligro.Validators;

public sealed class CrearPeligroValidator : AbstractValidator<CrearPeligroRequestDto>
{
    public CrearPeligroValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20).Matches("^[a-z0-9_]+$")
            .WithMessage("El código solo admite minúsculas, números y guion bajo.");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Descripcion).MaximumLength(2000);
        RuleFor(x => x.Clasificacion).IsInEnum();
    }
}
