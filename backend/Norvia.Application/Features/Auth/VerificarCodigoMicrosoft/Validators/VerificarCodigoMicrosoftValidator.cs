using FluentValidation;
using Norvia.Application.Features.Auth.VerificarCodigoMicrosoft.DTOs;

namespace Norvia.Application.Features.Auth.VerificarCodigoMicrosoft.Validators;

public sealed class VerificarCodigoMicrosoftValidator : AbstractValidator<VerificarCodigoMicrosoftRequestDto>
{
    public VerificarCodigoMicrosoftValidator()
    {
        RuleFor(x => x.TokenIdentidad).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Escribe el código de 6 dígitos.")
            .Matches(@"^\s*\d{6}\s*$").WithMessage("El código tiene 6 dígitos.");
    }
}
