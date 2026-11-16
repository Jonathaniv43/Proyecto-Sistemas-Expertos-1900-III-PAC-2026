using FluentValidation;
using Norvia.Application.Features.Catalogos.CambiarEstadoRubro.DTOs;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoRubro.Validators;

public sealed class CambiarEstadoRubroValidator : AbstractValidator<CambiarEstadoRubroRequestDto>
{
    public CambiarEstadoRubroValidator()
    {
        RuleFor(x => x.Activo).NotNull();
    }
}
