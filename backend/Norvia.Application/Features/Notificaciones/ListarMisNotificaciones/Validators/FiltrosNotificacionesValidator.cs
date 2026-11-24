using FluentValidation;
using Norvia.Application.Features.Notificaciones.ListarMisNotificaciones.DTOs;

namespace Norvia.Application.Features.Notificaciones.ListarMisNotificaciones.Validators;

public sealed class FiltrosNotificacionesValidator : AbstractValidator<FiltrosNotificacionesDto>
{
    public FiltrosNotificacionesValidator()
    {
        RuleFor(x => x.Limite).InclusiveBetween(1, FiltrosNotificacionesDto.LimiteMaximo).When(x => x.Limite is not null)
            .WithMessage($"El límite va de 1 a {FiltrosNotificacionesDto.LimiteMaximo}.");
    }
}
