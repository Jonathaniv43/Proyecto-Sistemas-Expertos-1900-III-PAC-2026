namespace Norvia.Application.Features.Auth.IdentidadMicrosoft.DTOs;

/// <param name="TokenIdentidad">Token firmado de corta vida para los pasos siguientes.</param>
/// <param name="Estado">Ver <see cref="EstadosIdentidad"/>.</param>
/// <param name="PersonaExistente">true si los nombres vienen de la persona ya guardada (RN-85) y no se pueden editar.</param>
public sealed record IdentidadMicrosoftResponseDto(
    string TokenIdentidad,
    string Email,
    string PrimerNombre,
    string? SegundoNombre,
    string PrimerApellido,
    string? SegundoApellido,
    string Estado,
    bool PersonaExistente);

public static class EstadosIdentidad
{
    /// <summary>El correo no tiene cuenta: ir al registro de empresa con los datos prellenados.</summary>
    public const string SinCuenta = "sin_cuenta";

    /// <summary>Esa cuenta de Microsoft ya está vinculada: puede iniciar sesión.</summary>
    public const string CuentaVinculada = "cuenta_vinculada";

    /// <summary>El correo tiene cuenta con contraseña sin vincular a Microsoft: debe confirmar la contraseña (RN-83).</summary>
    public const string CuentaConContrasena = "cuenta_con_contrasena";
}
