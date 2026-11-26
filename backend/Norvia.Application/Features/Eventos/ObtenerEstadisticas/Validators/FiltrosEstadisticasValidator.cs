using FluentValidation;
using Norvia.Application.Features.Eventos.ObtenerEstadisticas.DTOs;

namespace Norvia.Application.Features.Eventos.ObtenerEstadisticas.Validators;

public sealed class FiltrosEstadisticasValidator : AbstractValidator<FiltrosEstadisticasDto>
{
    /// <summary>Hasta dos años: el desglose por mes se sigue leyendo de un vistazo.</summary>
    public const int MesesMaximos = 24;

    public FiltrosEstadisticasValidator()
    {
        RuleFor(x => x.Desde).NotNull().WithMessage("Indique desde qué fecha.");
        RuleFor(x => x.Hasta).NotNull().WithMessage("Indique hasta qué fecha.");
        RuleFor(x => x).Must(x => x.Desde is null || x.Hasta is null || x.Desde <= x.Hasta)
            .WithName("Hasta").WithMessage("La fecha final no puede ser anterior a la inicial.");
        RuleFor(x => x).Must(x => x.Desde is not { } d || x.Hasta is not { } h || h <= d.AddMonths(MesesMaximos).AddDays(-1))
            .WithName("Hasta").WithMessage($"El periodo puede abarcar como máximo {MesesMaximos} meses.");
    }
}
