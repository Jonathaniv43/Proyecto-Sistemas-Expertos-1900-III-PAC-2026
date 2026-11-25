using FluentValidation;
using Norvia.Application.Features.Documentos.ListarVigentes.DTOs;

namespace Norvia.Application.Features.Documentos.ListarVigentes.Validators;

public sealed class FiltrosVigentesValidator : AbstractValidator<FiltrosVigentesDto>
{
    public FiltrosVigentesValidator()
    {
        RuleFor(x => x.Buscar).MaximumLength(100);
    }
}
