using FluentValidation;
using Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa.DTOs;

namespace Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa.Validators;

public sealed class ActualizarPerfilEmpresaValidator : AbstractValidator<ActualizarPerfilEmpresaRequestDto>
{
    public ActualizarPerfilEmpresaValidator()
    {
        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social no puede quedar vacía.")
            .MaximumLength(200).WithMessage("La razón social no puede exceder 200 caracteres.")
            .When(x => x.RazonSocial is not null);

        RuleFor(x => x.Direccion)
            .MaximumLength(300).WithMessage("La dirección no puede exceder 300 caracteres.")
            .When(x => x.Direccion is not null);

        RuleFor(x => x.ActividadEconomica)
            .MaximumLength(150).WithMessage("La actividad económica no puede exceder 150 caracteres.")
            .When(x => x.ActividadEconomica is not null);

        RuleFor(x => x.RubroCodigo)
            .NotEmpty().WithMessage("El código de rubro no puede quedar vacío.")
            .MaximumLength(30).WithMessage("El código de rubro no es válido.")
            .When(x => x.RubroCodigo is not null);

        RuleFor(x => x.TrabajadoresPermanentes)
            .GreaterThanOrEqualTo(0).WithMessage("El número de trabajadores permanentes no puede ser negativo.")
            .When(x => x.TrabajadoresPermanentes is not null);

        RuleFor(x => x).Must(x =>
                x.RazonSocial is not null
                || x.Direccion is not null
                || x.ActividadEconomica is not null
                || x.RubroCodigo is not null
                || x.TrabajadoresPermanentes is not null
                || x.FechaInicioOperaciones is not null)
            .WithName("Solicitud")
            .WithMessage("Indique al menos un dato del perfil que desea actualizar.");
    }
}
