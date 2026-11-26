using FluentValidation;
using Norvia.Application.Features.Encuestas.ResponderEncuesta.DTOs;

namespace Norvia.Application.Features.Encuestas.ResponderEncuesta.Validators;

public sealed class ResponderEncuestaValidator : AbstractValidator<ResponderEncuestaRequestDto>
{
    public ResponderEncuestaValidator()
    {
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.Respuestas)
            .NotNull()
            .Must(r => r is not null && r.Count > 0)
            .WithMessage("Envíe al menos una respuesta.")
            .Must(r => r is null || r.Select(x => x.PreguntaId).Distinct().Count() == r.Count)
            .WithMessage("No envíe la misma pregunta más de una vez.");

        RuleForEach(x => x.Respuestas).ChildRules(item =>
        {
            item.RuleFor(x => x.PreguntaId).NotEmpty();
            item.RuleFor(x => x)
                .Must(TieneExactamenteUnValor)
                .WithMessage(
                    "Cada respuesta debe enviar solo uno de: opcionId, textoLibre u opcionIds.");
        });
    }

    private static bool TieneExactamenteUnValor(RespuestaPreguntaRequestDto x)
    {
        var tieneOpcion = x.OpcionId is not null;
        var tieneTexto = !string.IsNullOrWhiteSpace(x.TextoLibre);
        var tieneMultiples = x.OpcionIds is { Count: > 0 };
        return (tieneOpcion ? 1 : 0) + (tieneTexto ? 1 : 0) + (tieneMultiples ? 1 : 0) == 1;
    }
}
