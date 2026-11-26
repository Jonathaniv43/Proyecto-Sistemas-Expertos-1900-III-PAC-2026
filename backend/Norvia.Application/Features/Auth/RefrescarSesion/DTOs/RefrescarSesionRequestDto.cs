namespace Norvia.Application.Features.Auth.RefrescarSesion.DTOs;

public sealed record RefrescarSesionRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
