using FluentValidation;
using Norvia.Application.Features.Encuestas.ListarHistorialEncuestas.DTOs;

namespace Norvia.Application.Features.Encuestas.ListarHistorialEncuestas.Validators;

public sealed class FiltrosHistorialEncuestasValidator : AbstractValidator<FiltrosHistorialEncuestasDto>
{
    public FiltrosHistorialEncuestasValidator()
    {
        RuleFor(x => x.Buscar).MaximumLength(100);
    }
}
