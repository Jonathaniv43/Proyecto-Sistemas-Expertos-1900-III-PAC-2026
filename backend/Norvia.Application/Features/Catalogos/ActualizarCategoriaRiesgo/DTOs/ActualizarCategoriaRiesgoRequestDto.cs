namespace Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo.DTOs;

/// <summary>Edición parcial: lo que no se envía se conserva. El estado va por /estado.</summary>
public sealed record ActualizarCategoriaRiesgoRequestDto
{
    /// <summary>Mueve la categoría a otro rubro activo.</summary>
    public short? RubroId { get; init; }
    public string? Nombre { get; init; }

    /// <summary>Texto vacío ("") borra la descripción.</summary>
    public string? Descripcion { get; init; }
}
