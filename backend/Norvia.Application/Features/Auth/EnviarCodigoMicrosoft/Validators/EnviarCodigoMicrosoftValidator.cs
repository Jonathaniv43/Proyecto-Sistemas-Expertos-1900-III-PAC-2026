using FluentValidation;
using Norvia.Application.Features.Auth.EnviarCodigoMicrosoft.DTOs;

namespace Norvia.Application.Features.Auth.EnviarCodigoMicrosoft.Validators;

public sealed class EnviarCodigoMicrosoftValidator : AbstractValidator<EnviarCodigoMicrosoftRequestDto>
{
    public EnviarCodigoMicrosoftValidator()
    {
        RuleFor(x => x.TokenIdentidad).NotEmpty().MaximumLength(2048);
    }
}
