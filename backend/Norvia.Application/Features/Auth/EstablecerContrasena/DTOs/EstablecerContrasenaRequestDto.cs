namespace Norvia.Application.Features.Auth.EstablecerContrasena.DTOs;

public sealed record EstablecerContrasenaRequestDto
{
    /// <summary>La contraseña con la que se inició sesión (la temporal, en el primer ingreso).</summary>
    public string ContrasenaActual { get; init; } = string.Empty;
    public string ContrasenaNueva { get; init; } = string.Empty;
}
