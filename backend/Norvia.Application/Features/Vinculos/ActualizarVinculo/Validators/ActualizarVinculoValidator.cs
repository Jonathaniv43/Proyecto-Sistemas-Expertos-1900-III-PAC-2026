using FluentValidation;
using Norvia.Application.Features.Vinculos.ActualizarVinculo.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.ActualizarVinculo.Validators;

public sealed class ActualizarVinculoValidator : AbstractValidator<ActualizarVinculoRequestDto>
{
    public ActualizarVinculoValidator()
    {
        RuleFor(x => x.RolCodigo).NotEmpty().When(x => x.RolCodigo is not null);
        RuleFor(x => x.Estado).IsInEnum();
        RuleFor(x => x.MotivoCodigo).IsInEnum();
        RuleFor(x => x.MotivoCodigo).NotNull()
            .When(x => x.Estado is EstadoVinculo.Suspendido or EstadoVinculo.Baja)
            .WithMessage("Indique el motivo para suspender o dar de baja el acceso.");
        RuleFor(x => x.MotivoDetalle).MaximumLength(1000);
        RuleFor(x => x.MotivoDetalle).NotEmpty()
            .When(x => x.MotivoCodigo == MotivoEstadoVinculo.Otro)
            .WithMessage("Describa el motivo cuando elige \"otro\".");
        RuleFor(x => x).Must(x => x.RolCodigo is not null || x.Estado is not null)
            .WithName("Solicitud").WithMessage("Indique el rol o el estado que desea cambiar.");
    }
}
