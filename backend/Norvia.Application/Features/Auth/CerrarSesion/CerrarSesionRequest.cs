namespace Norvia.Application.Features.Auth.CerrarSesion;

public sealed record CerrarSesionRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}
