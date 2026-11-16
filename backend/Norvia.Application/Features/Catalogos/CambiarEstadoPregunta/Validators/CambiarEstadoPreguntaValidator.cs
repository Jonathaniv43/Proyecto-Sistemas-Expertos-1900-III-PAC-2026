using FluentValidation;
using Norvia.Application.Features.Catalogos.CambiarEstadoPregunta.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPregunta.Validators;

public sealed class CambiarEstadoPreguntaValidator : AbstractValidator<CambiarEstadoPreguntaRequestDto>
{
    public CambiarEstadoPreguntaValidator()
    {
        RuleFor(x => x.Activo).NotNull();
    }
}
