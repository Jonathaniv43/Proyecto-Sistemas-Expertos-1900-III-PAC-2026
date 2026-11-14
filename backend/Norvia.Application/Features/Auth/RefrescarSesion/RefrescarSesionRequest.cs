namespace Norvia.Application.Features.Auth.RefrescarSesion;

public sealed record RefrescarSesionRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
