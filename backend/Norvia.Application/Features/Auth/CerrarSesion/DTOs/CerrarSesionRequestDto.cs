namespace Norvia.Application.Features.Auth.CerrarSesion.DTOs;

public sealed record CerrarSesionRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
