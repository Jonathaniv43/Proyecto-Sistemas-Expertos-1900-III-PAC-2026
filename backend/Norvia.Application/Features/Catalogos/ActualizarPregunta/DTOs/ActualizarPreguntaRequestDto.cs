namespace Norvia.Application.Features.Catalogos.ActualizarPregunta.DTOs;

/// <summary>
/// Edición parcial: lo que no se envía se conserva. El estado va por /estado.
/// <c>tipoRespuestaCodigo</c>, <c>peligroId</c> y <c>opciones</c> forman un bloque.
/// </summary>
public sealed record ActualizarPreguntaRequestDto : IRespuestaPregunta
{
    public Guid? CategoriaRiesgoId { get; init; }
    public string? Texto { get; init; }
    public bool? Obligatoria { get; init; }

    /// <summary>Posición dentro de la categoría. Si no se envía, se conserva (o va al final si cambia de categoría).</summary>
    public short? Orden { get; init; }

    public string? TipoRespuestaCodigo { get; init; }
    public Guid? PeligroId { get; init; }
    public IReadOnlyList<OpcionPreguntaRequestDto>? Opciones { get; init; }

    public bool CambiaRespuesta =>
        TipoRespuestaCodigo is not null || PeligroId is not null || Opciones is not null;
}
