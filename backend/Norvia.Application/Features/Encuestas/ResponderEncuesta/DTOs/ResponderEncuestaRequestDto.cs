namespace Norvia.Application.Features.Encuestas.ResponderEncuesta.DTOs;

public sealed record RespuestaPreguntaRequestDto
{
    public Guid PreguntaId { get; init; }
    public Guid? OpcionId { get; init; }
    public string? TextoLibre { get; init; }
    public IReadOnlyList<Guid>? OpcionIds { get; init; }
}

public sealed record ResponderEncuestaRequestDto
{
    public Guid VersionId { get; init; }
    public IReadOnlyList<RespuestaPreguntaRequestDto> Respuestas { get; init; } = [];
}

public sealed record ResponderEncuestaResponseDto(string Mensaje);