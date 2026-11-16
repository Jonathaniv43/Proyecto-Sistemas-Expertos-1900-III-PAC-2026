namespace Norvia.Application.Features.Catalogos.CambiarEstadoPeligro.DTOs;

public sealed record CambiarEstadoPeligroRequestDto
{
    /// <summary><c>true</c> activa, <c>false</c> desactiva. No hay borrado físico.</summary>
    public bool? Activo { get; init; }
}
