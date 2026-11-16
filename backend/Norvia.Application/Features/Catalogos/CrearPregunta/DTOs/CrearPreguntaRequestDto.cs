using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.CrearPregunta.DTOs;

/// <summary>La pregunta se agrega al final de su categoría.</summary>
public sealed record CrearPreguntaRequestDto : IRespuestaPregunta
{
    public Guid CategoriaRiesgoId { get; init; }
    public string Texto { get; init; } = string.Empty;
    public TipoRespuestaPregunta? TipoRespuesta { get; init; }
    public bool Obligatoria { get; init; } = true;

    /// <summary>Peligro que detecta; solo en preguntas de selección.</summary>
    public Guid? PeligroId { get; init; }

    /// <summary>Vacía en <c>respuesta_breve</c>; de 2 a 20 en las de selección.</summary>
    public IReadOnlyList<OpcionPreguntaRequestDto>? Opciones { get; init; } = [];
}
