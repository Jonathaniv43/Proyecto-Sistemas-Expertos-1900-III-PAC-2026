namespace Norvia.Application.Features.Auth.ConfirmarCambioContrasena.DTOs;

public sealed record ConfirmarCambioContrasenaRequestDto
{
    /// <summary>El token del enlace que llegó al correo.</summary>
    public string Token { get; init; } = string.Empty;
    public string ContrasenaNueva { get; init; } = string.Empty;
}
