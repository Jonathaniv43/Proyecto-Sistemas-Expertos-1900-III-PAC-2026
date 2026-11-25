using System.Text.RegularExpressions;
using FluentValidation;
using Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft.DTOs;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresaMicrosoft.Validators;

public sealed class RegistrarEmpresaMicrosoftValidator : AbstractValidator<RegistrarEmpresaMicrosoftRequestDto>
{
    private static readonly string[] TiposDocumento = ["dni", "pasaporte"];

    public RegistrarEmpresaMicrosoftValidator()
    {
        RuleFor(x => x.TokenIdentidad).NotEmpty().MaximumLength(2048);

        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social es obligatoria.")
            .MaximumLength(200).WithMessage("La razón social no puede exceder 200 caracteres.");

        RuleFor(x => x.RTN)
            .NotEmpty().WithMessage("El RTN es obligatorio.")
            .Matches("^[0-9]{14}$").WithMessage("El RTN debe contener exactamente 14 dígitos numéricos.");

        RuleFor(x => x.RubroCodigo)
            .NotEmpty().WithMessage("El rubro es obligatorio.")
            .MaximumLength(30).WithMessage("El código de rubro no es válido.");

        RuleFor(x => x.PrimerNombre)
            .NotEmpty().WithMessage("El primer nombre es obligatorio.")
            .MaximumLength(80).WithMessage("El primer nombre no puede exceder 80 caracteres.");
        RuleFor(x => x.SegundoNombre).MaximumLength(80).WithMessage("El segundo nombre no puede exceder 80 caracteres.");
        RuleFor(x => x.PrimerApellido)
            .NotEmpty().WithMessage("El primer apellido es obligatorio.")
            .MaximumLength(80).WithMessage("El primer apellido no puede exceder 80 caracteres.");
        RuleFor(x => x.SegundoApellido).MaximumLength(80).WithMessage("El segundo apellido no puede exceder 80 caracteres.");

        // Documento opcional, pero tipo y número van juntos.
        RuleFor(x => x.TipoDocumento)
            .Must(t => t is null || TiposDocumento.Contains(t.Trim().ToLowerInvariant()))
            .WithMessage("El tipo de documento debe ser 'dni' o 'pasaporte'.");
        RuleFor(x => x.NumeroDocumento).MaximumLength(50).WithMessage("El número de documento no puede exceder 50 caracteres.");
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.TipoDocumento) == string.IsNullOrWhiteSpace(x.NumeroDocumento))
            .WithMessage("Indica el tipo y el número de documento, o ninguno de los dos.");
        RuleFor(x => x)
            .Must(x => Regex.IsMatch(x.NumeroDocumento!.Trim(), @"^\d{13}$"))
            .WithMessage("El DNI debe tener 13 dígitos numéricos.")
            .When(x => x.TipoDocumento?.Trim().ToLowerInvariant() == "dni" && !string.IsNullOrWhiteSpace(x.NumeroDocumento));
        RuleFor(x => x)
            .Must(x => Regex.IsMatch(x.NumeroDocumento!.Trim(), @"^[A-Za-z0-9\-]+$"))
            .WithMessage("El pasaporte solo puede contener letras, números y guiones.")
            .When(x => x.TipoDocumento?.Trim().ToLowerInvariant() == "pasaporte" && !string.IsNullOrWhiteSpace(x.NumeroDocumento));
    }
}
