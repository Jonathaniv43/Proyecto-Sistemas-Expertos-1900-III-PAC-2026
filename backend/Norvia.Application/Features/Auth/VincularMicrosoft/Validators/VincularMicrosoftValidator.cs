using FluentValidation;
using Norvia.Application.Features.Auth.VincularMicrosoft.DTOs;

namespace Norvia.Application.Features.Auth.VincularMicrosoft.Validators;

public sealed class VincularMicrosoftValidator : AbstractValidator<VincularMicrosoftRequestDto>
{
    public VincularMicrosoftValidator()
    {
        RuleFor(x => x.TokenIdentidad).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}
