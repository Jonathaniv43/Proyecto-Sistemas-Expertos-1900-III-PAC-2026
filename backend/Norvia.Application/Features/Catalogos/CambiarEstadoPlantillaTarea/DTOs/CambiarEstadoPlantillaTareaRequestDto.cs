namespace Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaTarea.DTOs;

public sealed record CambiarEstadoPlantillaTareaRequestDto
{
    /// <summary><c>true</c> activa, <c>false</c> desactiva. No hay borrado físico.</summary>
    public bool? Activo { get; init; }
}
