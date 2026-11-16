namespace Norvia.Application.Features.Catalogos.CambiarEstadoRubro.DTOs;

public sealed record CambiarEstadoRubroRequestDto
{
    /// <summary><c>true</c> activa, <c>false</c> desactiva. No hay borrado físico.</summary>
    public bool? Activo { get; init; }
}
