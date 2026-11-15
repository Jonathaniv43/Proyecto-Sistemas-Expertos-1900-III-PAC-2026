using FluentValidation;

namespace Norvia.Application.Features.Empresas.RegistrarEmpresa;

public sealed class RegistrarEmpresaValidator : AbstractValidator<RegistrarEmpresaRequest>
{
    public RegistrarEmpresaValidator()
    {
        RuleFor(x => x.RazonSocial)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.RTN)
            .NotEmpty()
            .Length(14)
            .Matches("^[0-9]{14}$")
            .WithMessage("El RTN debe contener exactamente 14 dígitos numéricos.");

        RuleFor(x => x.PrimerNombre)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.SegundoNombre)
            .MaximumLength(80);

        RuleFor(x => x.PrimerApellido)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.SegundoApellido)
            .MaximumLength(80);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(254)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(15)
            .MaximumLength(64);
    }
}
