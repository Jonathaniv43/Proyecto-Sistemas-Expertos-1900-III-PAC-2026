namespace Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento.DTOs;

public sealed record CambiarEstadoPlantillaDocumentoRequestDto
{
    /// <summary><c>true</c> activa, <c>false</c> desactiva. No hay borrado físico.</summary>
    public bool? Activo { get; init; }
}
