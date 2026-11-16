using FluentValidation;
using Norvia.Application.Features.Catalogos.CrearPlantillaDocumento.DTOs;

namespace Norvia.Application.Features.Catalogos.CrearPlantillaDocumento.Validators;

public sealed class CrearPlantillaDocumentoValidator : AbstractValidator<CrearPlantillaDocumentoRequestDto>
{
    public CrearPlantillaDocumentoValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(30).Matches("^[a-z0-9_]+$")
            .WithMessage("El código solo admite minúsculas, números y guion bajo.");
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TipoDocumento).IsInEnum();
        RuleFor(x => x.RubroId).GreaterThan((short)0);
        RuleFor(x => x.Contenido).NotEmpty().MaximumLength(100_000);
    }
}
