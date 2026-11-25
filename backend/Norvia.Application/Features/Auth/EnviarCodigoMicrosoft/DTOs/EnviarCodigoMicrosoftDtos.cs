namespace Norvia.Application.Features.Auth.EnviarCodigoMicrosoft.DTOs;

public sealed record EnviarCodigoMicrosoftRequestDto
{
    public string TokenIdentidad { get; init; } = string.Empty;
}

public sealed record EnviarCodigoMicrosoftResponseDto(string Mensaje, int MinutosVigencia);
