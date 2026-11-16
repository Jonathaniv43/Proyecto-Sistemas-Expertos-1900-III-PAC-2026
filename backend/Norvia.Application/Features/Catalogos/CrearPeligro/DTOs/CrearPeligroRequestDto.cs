using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.CrearPeligro.DTOs;

public sealed record CrearPeligroRequestDto
{
    /// <summary>Identificador estable en minúsculas. No se puede cambiar después.</summary>
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public ClasificacionPeligro Clasificacion { get; init; }
}
