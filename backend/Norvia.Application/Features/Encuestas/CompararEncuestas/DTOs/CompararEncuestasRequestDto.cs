namespace Norvia.Application.Features.Encuestas.CompararEncuestas.DTOs;

public sealed record CompararEncuestasRequestDto(
    Guid? BaseVersionId,
    Guid? CompararVersionId);
