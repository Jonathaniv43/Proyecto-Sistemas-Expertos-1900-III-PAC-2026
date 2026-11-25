namespace Norvia.Application.Features.Auth.VincularMicrosoft.DTOs;

public sealed record VincularMicrosoftRequestDto
{
    public string TokenIdentidad { get; init; } = string.Empty;

    /// <summary>Contraseña actual del Acceso con ese correo (RN-83).</summary>
    public string Password { get; init; } = string.Empty;
}
