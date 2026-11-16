namespace Norvia.Application.Features.Auth.DTOs;

/// <summary>Respuesta compartida por los slices que abren o renuevan sesión.</summary>
/// <param name="DebeCambiarPassword">
/// true si la cuenta tiene una contraseña temporal (HU-020, HU-115): el front debe pedir el cambio
/// con <c>POST api/auth/contrasena/establecer</c> antes de continuar. La API no lo bloquea.
/// </param>
public sealed record SesionResponseDto(string AccessToken, string RefreshToken, int ExpiresIn, bool DebeCambiarPassword);
