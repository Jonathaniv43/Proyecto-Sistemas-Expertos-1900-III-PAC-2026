namespace Norvia.Application.Features.Catalogos.CambiarEstadoCategoriaRiesgo.DTOs;

public sealed record CambiarEstadoCategoriaRiesgoRequestDto
{
    /// <summary><c>true</c> activa, <c>false</c> desactiva. No hay borrado físico.</summary>
    public bool? Activo { get; init; }
}
