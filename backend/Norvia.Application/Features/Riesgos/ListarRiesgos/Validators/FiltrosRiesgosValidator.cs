using FluentValidation;
using Norvia.Application.Features.Riesgos.ListarRiesgos.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.ListarRiesgos.Validators;

public sealed class FiltrosRiesgosValidator : AbstractValidator<FiltrosRiesgosDto>
{
    public FiltrosRiesgosValidator()
    {
        RuleFor(x => x.Buscar).MaximumLength(100);
        RuleFor(x => x.Probabilidad).InclusiveBetween((short)1, (short)5).When(x => x.Probabilidad is not null);
        RuleFor(x => x.Severidad).InclusiveBetween((short)1, (short)5).When(x => x.Severidad is not null);
        RuleFor(x => x).Must(x => (x.Probabilidad is null) == (x.Severidad is null))
            .WithName("Celda").WithMessage("Indique la probabilidad y la severidad de la celda, o ninguna.");
        RuleFor(x => x).Must(x => !(x.SinEvaluar == true && (x.Nivel is not null || x.Probabilidad is not null)))
            .WithName("Nivel").WithMessage("Un riesgo sin evaluar no tiene nivel ni celda.");
        RuleFor(x => x).Must(x => !(x.SinControl == true && (x.SinEvaluar == true || x.Nivel is ClasificacionRiesgo.Bajo or ClasificacionRiesgo.Medio)))
            .WithName("SinControl").WithMessage("Solo los riesgos altos o críticos se señalan sin control.");
    }
}
