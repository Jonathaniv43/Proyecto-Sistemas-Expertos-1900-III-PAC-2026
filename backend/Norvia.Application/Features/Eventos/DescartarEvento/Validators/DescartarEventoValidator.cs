using FluentValidation;
using Norvia.Application.Features.Eventos.DescartarEvento.DTOs;

namespace Norvia.Application.Features.Eventos.DescartarEvento.Validators;

public sealed class DescartarEventoValidator : AbstractValidator<DescartarEventoRequestDto>
{
    public DescartarEventoValidator()
    {
        RuleFor(x => x.Motivo).MaximumLength(1000);
    }
}
