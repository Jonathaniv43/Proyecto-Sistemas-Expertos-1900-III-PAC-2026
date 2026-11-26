using FluentValidation;
using Norvia.Application.Features.Encuestas.CompararEncuestas.DTOs;

namespace Norvia.Application.Features.Encuestas.CompararEncuestas.Validators;

public sealed class CompararEncuestasValidator : AbstractValidator<CompararEncuestasRequestDto>
{
    public CompararEncuestasValidator()
    {
        RuleFor(x => x.BaseVersionId).NotEmpty();
        RuleFor(x => x.CompararVersionId).NotEmpty();
        RuleFor(x => x)
            .Must(x => !x.BaseVersionId.HasValue || !x.CompararVersionId.HasValue || x.BaseVersionId.Value != x.CompararVersionId.Value)
            .WithMessage("Debes elegir dos versiones distintas para compararlas.");
    }
}
