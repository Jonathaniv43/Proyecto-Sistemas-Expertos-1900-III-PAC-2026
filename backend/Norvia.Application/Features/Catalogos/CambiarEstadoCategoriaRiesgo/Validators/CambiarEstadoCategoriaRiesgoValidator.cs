using FluentValidation;
using Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo.Validators;

public sealed class CambiarEstadoCategoriaRiesgoValidator : AbstractValidator<CambiarEstadoCategoriaRiesgoRequestDto>
{
    public CambiarEstadoCategoriaRiesgoValidator()
    {
        RuleFor(x => x.Activo).NotNull();
    }
}
