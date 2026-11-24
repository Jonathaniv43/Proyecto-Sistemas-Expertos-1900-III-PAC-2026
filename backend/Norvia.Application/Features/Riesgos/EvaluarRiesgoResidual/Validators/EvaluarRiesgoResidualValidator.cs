using FluentValidation;
using Norvia.Application.Features.Riesgos.EvaluarRiesgoResidual.DTOs;

namespace Norvia.Application.Features.Riesgos.EvaluarRiesgoResidual.Validators;

public sealed class EvaluarRiesgoResidualValidator : AbstractValidator<EvaluarRiesgoResidualRequestDto>
{
    public EvaluarRiesgoResidualValidator()
    {
        RuleFor(x => x.Probabilidad).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Indique la probabilidad.")
            .InclusiveBetween((short)1, (short)5).WithMessage("La probabilidad va del 1 al 5.");
        RuleFor(x => x.Severidad).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Indique la severidad.")
            .InclusiveBetween((short)1, (short)5).WithMessage("La severidad va del 1 al 5.");
    }
}
