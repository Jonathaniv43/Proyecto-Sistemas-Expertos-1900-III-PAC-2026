using FluentValidation;
using Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento.Validators;

public sealed class CambiarEstadoPlantillaDocumentoValidator : AbstractValidator<CambiarEstadoPlantillaDocumentoRequestDto>
{
    public CambiarEstadoPlantillaDocumentoValidator()
    {
        RuleFor(x => x.Activo).NotNull();
    }
}
