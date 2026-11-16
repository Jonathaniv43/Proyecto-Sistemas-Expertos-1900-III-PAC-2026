using FluentValidation;
using Norvia.Application.Common.Validacion;
using Norvia.Application.Features.Vinculos.CrearVinculo.DTOs;

namespace Norvia.Application.Features.Vinculos.CrearVinculo.Validators;

public sealed class CrearVinculoValidator : AbstractValidator<CrearVinculoRequestDto>
{
    public CrearVinculoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(x => x.PrimerNombre).NotEmpty().MaximumLength(80);
        RuleFor(x => x.SegundoNombre).MaximumLength(80);
        RuleFor(x => x.PrimerApellido).NotEmpty().MaximumLength(80);
        RuleFor(x => x.SegundoApellido).MaximumLength(80);
        RuleFor(x => x.RolCodigo).NotEmpty();

        // Si hace falta (cuenta nueva) lo decide el servicio; si viene, debe cumplir la política.
        RuleFor(x => x.PasswordTemporal!).CumplePoliticaContrasena().When(x => x.PasswordTemporal is not null);
    }
}
