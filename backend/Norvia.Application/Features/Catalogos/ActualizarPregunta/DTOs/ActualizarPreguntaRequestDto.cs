using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.ActualizarPregunta.DTOs;

/// <summary>
/// Edición parcial: lo que no se envía se conserva. El estado va por /estado.
/// <para>
/// <c>tipoRespuesta</c>, <c>peligroId</c> y <c>opciones</c> forman un bloque: para cambiar cualquiera de
/// ellos se envían <c>tipoRespuesta</c> y <c>opciones</c> juntos, y entonces <c>peligroId</c> ausente
/// significa "sin peligro". Las opciones se reemplazan completas. Las encuestas ya generadas no cambian,
/// porque guardan su propia copia (RN-12).
/// </para>
/// </summary>
public sealed record ActualizarPreguntaRequestDto : IRespuestaPregunta
{
    public Guid? CategoriaRiesgoId { get; init; }
    public string? Texto { get; init; }
    public bool? Obligatoria { get; init; }

    /// <summary>Posición dentro de la categoría. Si no se envía, se conserva (o va al final si cambia de categoría).</summary>
    public short? Orden { get; init; }

    public TipoRespuestaPregunta? TipoRespuesta { get; init; }
    public Guid? PeligroId { get; init; }
    public IReadOnlyList<OpcionPreguntaRequestDto>? Opciones { get; init; }

    public bool CambiaRespuesta => TipoRespuesta is not null || PeligroId is not null || Opciones is not null;
}
