namespace Norvia.Application.Features.Auth.SolicitarCambioContrasena.DTOs;

public sealed record SolicitarCambioContrasenaRequestDto
{
    public string Email { get; init; } = string.Empty;
}
