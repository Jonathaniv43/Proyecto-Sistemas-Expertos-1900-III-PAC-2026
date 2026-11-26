namespace Norvia.Application.Features.Auth.ReestablecerContrasena.DTOs;

public sealed record ReestablecerContrasenaRequestDto
{
    /// <summary>La contraseña con la que se inició sesión (la temporal, en el primer ingreso).</summary>
    public string ContrasenaActual { get; init; } = string.Empty;
    public string ContrasenaNueva { get; init; } = string.Empty;
}