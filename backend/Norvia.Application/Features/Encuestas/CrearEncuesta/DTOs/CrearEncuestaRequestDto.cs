namespace Norvia.Application.Features.Encuestas.CrearEncuesta.DTOs;

public sealed record CrearEncuestaRequestDto
{
    public string Titulo { get; init; } = string.Empty;
    public string? Instrucciones { get; init; }
    public IReadOnlyList<Guid> CategoriaRiesgoIds { get; init; } = [];
}
