namespace Norvia.Application.Features.Auth.VerificarCuenta.DTOs;

/// <summary>
/// Confirma el correo con el código de 6 dígitos del alta y fija la contraseña definitiva (HU-114, RN-86).
/// Anónimo: no hay JWT hasta <c>Acceso.Verificado</c>.
/// </summary>
public sealed record VerificarCuentaRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public string ContrasenaNueva { get; init; } = string.Empty;
    public string ContrasenaNuevaRepetida { get; init; } = string.Empty;
}
