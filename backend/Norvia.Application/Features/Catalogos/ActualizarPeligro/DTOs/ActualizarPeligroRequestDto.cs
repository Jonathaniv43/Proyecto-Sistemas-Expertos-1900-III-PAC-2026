using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.ActualizarPeligro.DTOs;

/// <summary>Edición parcial: lo que no se envía se conserva. El código no se edita; el estado va por /estado.</summary>
public sealed record ActualizarPeligroRequestDto
{
    public string? Nombre { get; init; }

    /// <summary>Texto vacío ("") borra la descripción.</summary>
    public string? Descripcion { get; init; }
    public ClasificacionPeligro? Clasificacion { get; init; }
}
