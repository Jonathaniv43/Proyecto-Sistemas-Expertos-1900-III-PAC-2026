using FluentValidation;
using Norvia.Application.Features.Vinculos.CrearEncuestadosLote.DTOs;

namespace Norvia.Application.Features.Vinculos.CrearEncuestadosLote.Validators;

/// <summary>Solo la forma del lote. Cada fila se valida en el servicio para informar sus errores y seguir con las demás.</summary>
public sealed class CrearEncuestadosLoteValidator : AbstractValidator<CrearEncuestadosLoteRequestDto>
{
    public const int MaxFilas = 500;

    public CrearEncuestadosLoteValidator()
    {
        RuleFor(x => x.Filas).NotEmpty().WithMessage("Envíe al menos una fila.");
        RuleFor(x => x.Filas.Count).LessThanOrEqualTo(MaxFilas).When(x => x.Filas is not null)
            .OverridePropertyName("Filas").WithMessage($"Envíe como máximo {MaxFilas} filas por lote.");
    }
}

/// <summary>Reglas de una fila. Lo ejecuta el servicio, no el filtro de validación.</summary>
public sealed class FilaEncuestadoValidator : AbstractValidator<FilaEncuestadoDto>
{
    public FilaEncuestadoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Falta el correo.")
            .MaximumLength(254).EmailAddress().WithMessage("El correo no es válido.");
        RuleFor(x => x.PrimerNombre).NotEmpty().WithMessage("Falta el primer nombre.").MaximumLength(80);
        RuleFor(x => x.SegundoNombre).MaximumLength(80);
        RuleFor(x => x.PrimerApellido).NotEmpty().WithMessage("Falta el primer apellido.").MaximumLength(80);
        RuleFor(x => x.SegundoApellido).MaximumLength(80);
    }
}
