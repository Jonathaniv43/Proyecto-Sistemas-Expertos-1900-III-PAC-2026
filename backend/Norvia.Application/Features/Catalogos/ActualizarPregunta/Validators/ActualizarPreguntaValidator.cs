using FluentValidation;
using Norvia.Application.Features.Catalogos.ActualizarPregunta.DTOs;

namespace Norvia.Application.Features.Catalogos.ActualizarPregunta.Validators;

public sealed class ActualizarPreguntaValidator : AbstractValidator<ActualizarPreguntaRequestDto>
{
    public ActualizarPreguntaValidator()
    {
        RuleFor(x => x.CategoriaRiesgoId).NotEqual(Guid.Empty);
        RuleFor(x => x.Texto).NotEmpty().MaximumLength(500).When(x => x.Texto is not null);
        RuleFor(x => x.Orden).GreaterThan((short)0);

        // Tipo, peligro y opciones se validan juntos (ver el DTO).
        When(x => x.CambiaRespuesta, () => Include(new ReglasPregunta()));
    }
}
