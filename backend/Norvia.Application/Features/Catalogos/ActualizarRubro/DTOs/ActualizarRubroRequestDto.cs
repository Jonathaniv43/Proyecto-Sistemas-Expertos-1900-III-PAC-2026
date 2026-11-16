namespace Norvia.Application.Features.Catalogos.ActualizarRubro.DTOs;

/// <summary>Edición parcial: lo que no se envía se conserva. El código no se edita; el estado va por /estado.</summary>
public sealed record ActualizarRubroRequestDto
{
    public string? Nombre { get; init; }
}
