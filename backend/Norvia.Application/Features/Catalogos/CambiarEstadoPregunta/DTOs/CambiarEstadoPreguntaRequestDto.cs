namespace Norvia.Application.Features.Catalogos.CambiarEstadoPregunta.DTOs;

public sealed record CambiarEstadoPreguntaRequestDto
{
    /// <summary><c>true</c> activa, <c>false</c> desactiva. No hay borrado físico.</summary>
    public bool? Activo { get; init; }
}
