using FluentValidation;
using Norvia.Application.Features.Auth.DTOs;

namespace Norvia.Application.Features.Auth.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
