using FluentValidation;
using Norvia.Application.Features.Auth.DTOs;

namespace Norvia.Application.Features.Auth.Validators;

public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
