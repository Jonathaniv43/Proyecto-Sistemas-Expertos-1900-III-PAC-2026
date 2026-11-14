namespace Norvia.Application.Features.Auth;

/// <summary>Tokens que devuelven los slices que abren o renuevan una sesión.</summary>
public sealed record SesionResponse(string AccessToken, string RefreshToken, int ExpiresIn);
