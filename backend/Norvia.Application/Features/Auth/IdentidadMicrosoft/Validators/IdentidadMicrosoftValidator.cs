using FluentValidation;
using Norvia.Application.Features.Auth.IdentidadMicrosoft.DTOs;

namespace Norvia.Application.Features.Auth.IdentidadMicrosoft.Validators;

public sealed class IdentidadMicrosoftValidator : AbstractValidator<IdentidadMicrosoftRequestDto>
{
    public IdentidadMicrosoftValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(4096);
        RuleFor(x => x.CodeVerifier).NotEmpty().MinimumLength(43).MaximumLength(128);
        RuleFor(x => x.RedirectUri).NotEmpty().MaximumLength(500);
    }
}
