using FluentValidation;
using Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta.Validators;

public sealed class ActualizarEncuestaBorradorValidator : AbstractValidator<ActualizarEncuestaBorradorRequestDto>
{
    public const int MaxOpciones = 20;

    public ActualizarEncuestaBorradorValidator()
    {
        When(x => x.Titulo is not null, () =>
        {
            RuleFor(x => x.Titulo!).NotEmpty().MaximumLength(200);
        });

        RuleFor(x => x.Preguntas)
            .NotNull()
            .Must(ps => ps is not null && ps.Count > 0)
            .WithMessage("La encuesta debe incluir al menos una pregunta.")
            .Must(ps => ps is null || ps.Where(p => p is not null).Select(p => p.Orden).Distinct().Count() == ps.Count)
            .WithMessage("Dos preguntas no pueden tener la misma posición.");

        RuleForEach(x => x.Preguntas)
            .NotNull()
            .SetValidator(new PreguntaBorradorValidator());
    }

    private sealed class PreguntaBorradorValidator : AbstractValidator<PreguntaBorradorRequestDto>
    {
        public PreguntaBorradorValidator()
        {
            RuleFor(x => x.Texto).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Orden).GreaterThan((short)0);
            RuleFor(x => x.TipoRespuesta).NotNull().IsInEnum();
            RuleFor(x => x.Opciones).NotNull()
                .WithMessage("Envíe las opciones (lista vacía en una pregunta de respuesta breve).");
            RuleForEach(x => x.Opciones).NotNull();

            When(x => x.TipoRespuesta is not null && x.Opciones is not null && x.Opciones.All(o => o is not null),
                ReglasSegunTipo);
        }

        private void ReglasSegunTipo()
        {
            When(x => x.TipoRespuesta == TipoRespuestaPregunta.RespuestaBreve, () =>
            {
                RuleFor(x => x.Opciones).Empty()
                    .WithMessage("Una pregunta de respuesta breve no lleva opciones.");
                RuleFor(x => x.PeligroId).Null()
                    .WithMessage("Una pregunta de respuesta breve no puede indicar un peligro.");
            }).Otherwise(() =>
            {
                RuleFor(x => x.Opciones.Count).InclusiveBetween(2, MaxOpciones)
                    .OverridePropertyName("Opciones")
                    .WithMessage($"Una pregunta de selección necesita entre 2 y {MaxOpciones} opciones.");
                RuleForEach(x => x.Opciones).ChildRules(o =>
                {
                    o.RuleFor(x => x.Texto).NotEmpty().MaximumLength(200);
                    o.RuleFor(x => x.Orden).GreaterThan((short)0);
                });
                RuleFor(x => x.Opciones)
                    .Must(os => os.Select(o => o.Orden).Distinct().Count() == os.Count)
                    .WithMessage("Dos opciones no pueden tener la misma posición.")
                    .Must(os => os.Select(o => o.Texto.Trim().ToLowerInvariant()).Distinct().Count() == os.Count)
                    .WithMessage("Dos opciones no pueden tener el mismo texto.");

                RuleFor(x => x.Opciones)
                    .Must(os => os.Any(o => o.IndicaExposicion))
                    .When(x => x.PeligroId is not null)
                    .WithMessage("Marque al menos una opción que indique exposición al peligro.");
                RuleFor(x => x.Opciones)
                    .Must(os => os.All(o => !o.IndicaExposicion))
                    .When(x => x.PeligroId is null)
                    .WithMessage("Solo puede marcar opciones de exposición si la pregunta tiene un peligro asociado.");
            });
        }
    }
}
