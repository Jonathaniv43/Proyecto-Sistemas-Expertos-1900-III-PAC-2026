using FluentValidation;
using Norvia.Application.Features.Vinculos.CrearEncuestado.DTOs;

namespace Norvia.Application.Features.Vinculos.CrearEncuestado.Validators;

public sealed class CrearEncuestadoValidator : AbstractValidator<CrearEncuestadoRequestDto>
{
    public CrearEncuestadoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(x => x.PrimerNombre).NotEmpty().MaximumLength(80);
        RuleFor(x => x.SegundoNombre).MaximumLength(80);
        RuleFor(x => x.PrimerApellido).NotEmpty().MaximumLength(80);
        RuleFor(x => x.SegundoApellido).MaximumLength(80);
    }
}
