using FluentValidation;
using Norvia.Application.Features.Catalogos.CambiarEstadoPeligro.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPeligro.Validators;

public sealed class CambiarEstadoPeligroValidator : AbstractValidator<CambiarEstadoPeligroRequestDto>
{
    public CambiarEstadoPeligroValidator()
    {
        RuleFor(x => x.Activo).NotNull();
    }
}
