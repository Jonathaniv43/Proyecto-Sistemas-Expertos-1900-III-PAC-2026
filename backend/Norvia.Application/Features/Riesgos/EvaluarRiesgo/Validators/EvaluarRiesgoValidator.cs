using FluentValidation;
using Norvia.Application.Features.Riesgos.EvaluarRiesgo.DTOs;

namespace Norvia.Application.Features.Riesgos.EvaluarRiesgo.Validators;

public sealed class EvaluarRiesgoValidator : AbstractValidator<EvaluarRiesgoRequestDto>
{
    public EvaluarRiesgoValidator()
    {
        RuleFor(x => x.Probabilidad).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Indique la probabilidad.")
            .InclusiveBetween((short)1, (short)5).WithMessage("La probabilidad va del 1 al 5.");
        RuleFor(x => x.Severidad).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Indique la severidad.")
            .InclusiveBetween((short)1, (short)5).WithMessage("La severidad va del 1 al 5.");
    }
}
