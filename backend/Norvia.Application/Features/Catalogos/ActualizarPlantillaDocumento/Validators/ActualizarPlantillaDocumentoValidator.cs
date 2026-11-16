using FluentValidation;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento.Validators;

public sealed class ActualizarPlantillaDocumentoValidator : AbstractValidator<ActualizarPlantillaDocumentoRequestDto>
{
    public ActualizarPlantillaDocumentoValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200).When(x => x.Titulo is not null);
        RuleFor(x => x.TipoDocumento).IsInEnum();
        RuleFor(x => x.RubroId).GreaterThanOrEqualTo((short)0);
        RuleFor(x => x.Contenido).NotEmpty().MaximumLength(100_000).When(x => x.Contenido is not null);
    }
}
