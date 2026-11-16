namespace Norvia.Application.Features.Catalogos.CrearRubro.DTOs;

public sealed record CrearRubroRequestDto
{
    /// <summary>Identificador estable en minúsculas, por ejemplo <c>servicios</c>. No se puede cambiar después.</summary>
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
}
