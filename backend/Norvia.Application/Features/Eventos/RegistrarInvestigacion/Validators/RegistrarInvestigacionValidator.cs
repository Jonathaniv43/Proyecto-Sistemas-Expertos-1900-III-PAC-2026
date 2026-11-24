using FluentValidation;
using Norvia.Application.Features.Eventos.RegistrarInvestigacion.DTOs;

namespace Norvia.Application.Features.Eventos.RegistrarInvestigacion.Validators;

public sealed class RegistrarInvestigacionValidator : AbstractValidator<RegistrarInvestigacionRequestDto>
{
    public RegistrarInvestigacionValidator()
    {
        RuleFor(x => x.CausasInmediatas).Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Indique las causas inmediatas.")
            .MaximumLength(4000);
        RuleFor(x => x.CausasBasicas).Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Indique las causas de fondo.")
            .MaximumLength(4000);
        RuleFor(x => x.Conclusiones).MaximumLength(4000);
        RuleFor(x => x.InvestigadaPorId).Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Indique quién investigó.")
            .NotEqual(Guid.Empty).WithMessage("Indique quién investigó.");
        RuleFor(x => x.FechaInvestigacion).NotNull().WithMessage("Indique la fecha de la investigación.");
    }
}
