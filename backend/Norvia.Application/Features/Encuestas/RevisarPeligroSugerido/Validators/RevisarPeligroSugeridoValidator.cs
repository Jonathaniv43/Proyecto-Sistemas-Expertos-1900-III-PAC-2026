using FluentValidation;
using Norvia.Application.Features.Encuestas.RevisarPeligroSugerido.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.RevisarPeligroSugerido.Validators;

public sealed class RevisarPeligroSugeridoValidator : AbstractValidator<RevisarPeligroSugeridoRequestDto>
{
    public RevisarPeligroSugeridoValidator()
    {
        RuleFor(x => x.Estado).Cascade(CascadeMode.Stop).NotNull()
            .Must(e => e is EstadoRiesgo.Confirmado or EstadoRiesgo.Descartado)
            .WithMessage("Indique si la sugerencia se confirma o se descarta.");
        RuleFor(x => x.Actividad).MaximumLength(200);
        RuleFor(x => x.Actividad).Null()
            .When(x => x.Estado == EstadoRiesgo.Descartado)
            .WithMessage("La actividad solo se indica al confirmar el peligro.");
    }
}
