using FluentValidation;
using Norvia.Application.Features.Empresas.RegistrarEmpresa.DTOs;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresa.Validators;

public sealed class RegistrarEmpresaValidator : AbstractValidator<RegistrarEmpresaRequestDto>
{
    private static readonly string[] TiposDocumento = ["dni", "pasaporte"];

    public RegistrarEmpresaValidator()
    {
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

        RuleFor(x => x.SegundoNombre)
            .MaximumLength(80).WithMessage("El segundo nombre no puede exceder 80 caracteres.");

        RuleFor(x => x.PrimerApellido)
            .NotEmpty().WithMessage("El primer apellido es obligatorio.")
            .MaximumLength(80).WithMessage("El primer apellido no puede exceder 80 caracteres.");

        RuleFor(x => x.SegundoApellido)
            .MaximumLength(80).WithMessage("El segundo apellido no puede exceder 80 caracteres.");

        RuleFor(x => x.TipoDocumento)
            .NotEmpty().WithMessage("El tipo de documento es obligatorio.")
            .Must(t => TiposDocumento.Contains(t.Trim().ToLowerInvariant()))
            .WithMessage("El tipo de documento debe ser 'dni' o 'pasaporte'.");

        RuleFor(x => x.NumeroDocumento)
            .NotEmpty().WithMessage("El número de documento es obligatorio.")
            .MaximumLength(50).WithMessage("El número de documento no puede exceder 50 caracteres.");

        RuleFor(x => x)
            .Must(x => x.TipoDocumento.Trim().ToLowerInvariant() != "dni"
                       || System.Text.RegularExpressions.Regex.IsMatch(x.NumeroDocumento.Trim(), @"^\d{13}$"))
            .WithMessage("El DNI debe tener 13 dígitos numéricos.")
            .When(x => !string.IsNullOrWhiteSpace(x.TipoDocumento) && x.TipoDocumento.Trim().ToLowerInvariant() == "dni");

        RuleFor(x => x)
            .Must(x => System.Text.RegularExpressions.Regex.IsMatch(x.NumeroDocumento.Trim(), @"^[A-Za-z0-9\-]+$"))
            .WithMessage("El pasaporte solo puede contener letras, números y guiones.")
            .When(x => !string.IsNullOrWhiteSpace(x.TipoDocumento) && x.TipoDocumento.Trim().ToLowerInvariant() == "pasaporte");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .MaximumLength(254).WithMessage("El correo no puede exceder 254 caracteres.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.");
    }
}