using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento.DTOs;

/// <summary>Edición parcial: lo que no se envía se conserva. El código no se edita; el estado va por /estado.</summary>
public sealed record ActualizarPlantillaDocumentoRequestDto
{
    public string? Titulo { get; init; }
    public TipoDocumento? TipoDocumento { get; init; }

    /// <summary>Rubro al que aplica; <c>0</c> = todos los rubros.</summary>
    public short? RubroId { get; init; }
    public string? Contenido { get; init; }
}
