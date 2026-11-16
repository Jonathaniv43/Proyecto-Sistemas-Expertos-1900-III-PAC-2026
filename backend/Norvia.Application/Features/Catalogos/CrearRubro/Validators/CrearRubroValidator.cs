using FluentValidation;
using Norvia.Application.Features.Catalogos.CrearRubro.DTOs;

namespace Norvia.Application.Features.Catalogos.CrearRubro.Validators;

public sealed class CrearRubroValidator : AbstractValidator<CrearRubroRequestDto>
{
    public CrearRubroValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(30).Matches("^[a-z0-9_]+$")
            .WithMessage("El código solo admite minúsculas, números y guion bajo.");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(80);
    }
}
