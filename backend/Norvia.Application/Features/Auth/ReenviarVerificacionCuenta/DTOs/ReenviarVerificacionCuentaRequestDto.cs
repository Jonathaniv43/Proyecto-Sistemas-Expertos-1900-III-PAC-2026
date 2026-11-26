namespace Norvia.Application.Features.Auth.ReenviarVerificacionCuenta.DTOs;

public sealed record ReenviarVerificacionCuentaRequestDto
{
    public string Email { get; init; } = string.Empty;
}
