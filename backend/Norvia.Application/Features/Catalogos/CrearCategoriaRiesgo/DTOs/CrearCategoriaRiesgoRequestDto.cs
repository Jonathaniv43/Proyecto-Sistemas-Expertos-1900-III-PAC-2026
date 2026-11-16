namespace Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo.DTOs;

public sealed record CrearCategoriaRiesgoRequestDto
{
    public short RubroId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
}
