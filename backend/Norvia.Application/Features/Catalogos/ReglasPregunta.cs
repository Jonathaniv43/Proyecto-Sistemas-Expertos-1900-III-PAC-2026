using FluentValidation;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos;

/// <summary>Opción de una pregunta de selección, tal como la envía el cliente al crear o editar.</summary>
public sealed record OpcionPreguntaRequestDto
{
    public string Texto { get; init; } = string.Empty;
    public short Orden { get; init; }
    public bool IndicaExposicion { get; init; }
}

/// <summary>
/// Bloque de respuesta de una pregunta: tipo (código de catálogo), peligro y opciones (D26, RN-68).
/// </summary>
public interface IRespuestaPregunta
{
    string? TipoRespuestaCodigo { get; }
    Guid? PeligroId { get; }
    IReadOnlyList<OpcionPreguntaRequestDto>? Opciones { get; }
}

/// <summary>
/// Reglas del bloque de respuesta (D26, RN-68), compartidas por CrearPregunta y ActualizarPregunta.
/// </summary>
internal sealed class ReglasPregunta : AbstractValidator<IRespuestaPregunta>
{
    public const int MaxOpciones = 20;

    public ReglasPregunta()
    {
        RuleFor(x => x.TipoRespuestaCodigo)
            .NotEmpty()
            .Must(TipoRespuestaPreguntaCatalogo.EsCodigoConocido)
            .WithMessage("El tipo de respuesta no es válido.");
        RuleFor(x => x.Opciones).NotNull()
            .WithMessage("Envíe las opciones (lista vacía en una pregunta de respuesta breve).");
        RuleForEach(x => x.Opciones).NotNull();

        When(x => TipoRespuestaPreguntaCatalogo.EsCodigoConocido(x.TipoRespuestaCodigo)
                  && x.Opciones is not null && x.Opciones.All(o => o is not null),
            ReglasSegunTipo);
    }

    private void ReglasSegunTipo()
    {
        When(x => TipoRespuestaPreguntaCatalogo.EsCodigoRespuestaBreve(x.TipoRespuestaCodigo), () =>
        {
            RuleFor(x => x.Opciones).Empty()
                .WithMessage("Una pregunta de respuesta breve no lleva opciones.");
            RuleFor(x => x.PeligroId).Null()
                .WithMessage("Una pregunta de respuesta breve no puede indicar un peligro.");
        }).Otherwise(() =>
        {
            RuleFor(x => x.Opciones!.Count).InclusiveBetween(2, MaxOpciones)
                .OverridePropertyName("Opciones")
                .WithMessage($"Una pregunta de selección necesita entre 2 y {MaxOpciones} opciones.");
            RuleForEach(x => x.Opciones).ChildRules(o =>
            {
                o.RuleFor(x => x.Texto).NotEmpty().MaximumLength(200);
                o.RuleFor(x => x.Orden).GreaterThan((short)0);
            });
            RuleFor(x => x.Opciones)
                .Must(os => os!.Select(o => o.Orden).Distinct().Count() == os!.Count)
                .WithMessage("Dos opciones no pueden tener la misma posición.")
                .Must(os => os!.Select(o => o.Texto.Trim().ToLowerInvariant()).Distinct().Count() == os!.Count)
                .WithMessage("Dos opciones no pueden tener el mismo texto.");

            RuleFor(x => x.Opciones)
                .Must(os => os!.Any(o => o.IndicaExposicion))
                .When(x => x.PeligroId is not null)
                .WithMessage("Marque al menos una opción que indique exposición al peligro.");
            RuleFor(x => x.Opciones)
                .Must(os => os!.All(o => !o.IndicaExposicion))
                .When(x => x.PeligroId is null)
                .WithMessage("Solo puede marcar opciones de exposición si la pregunta tiene un peligro asociado.");
        });
    }
}
