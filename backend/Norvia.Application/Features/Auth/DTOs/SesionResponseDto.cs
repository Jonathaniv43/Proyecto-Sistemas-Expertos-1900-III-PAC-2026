namespace Norvia.Application.Features.Auth.DTOs;

/// <summary>Respuesta compartida por los slices que abren o renuevan sesión.</summary>
public sealed record SesionResponseDto(string AccessToken, string RefreshToken, int ExpiresIn);
