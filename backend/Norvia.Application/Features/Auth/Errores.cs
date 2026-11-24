using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Auth;

/// <summary>Errores de negocio del módulo Auth, compartidos solo por sus slices.</summary>
public static class Errores
{
    /// <summary>Mismo mensaje para cualquier dato incorrecto: no revela qué falló (HU-018).</summary>
    public static readonly Error CredencialesInvalidas =
        new("auth.credenciales_invalidas", "Las credenciales no son válidas.", TipoError.NoAutorizado);

    public static readonly Error SesionInvalida =
        new("auth.sesion_invalida", "La sesión no es válida o expiró.", TipoError.NoAutorizado);

    public static readonly Error ContrasenaActualIncorrecta =
        new("auth.contrasena_actual_incorrecta", "La contraseña actual no es correcta.", TipoError.Validacion);

    /// <summary>HU-022: el enlace del correo venció, ya se usó (la contraseña cambió) o fue alterado.</summary>
    public static readonly Error EnlaceCambioContrasenaInvalido =
        new("auth.enlace_invalido", "El enlace para cambiar la contraseña venció o ya se usó. Pide uno nuevo.", TipoError.Expirado);
    public static readonly Error ContrasenaIgualActual =
        new("auth.contrasena_igual_actual", "La contraseña nueva debe ser distinta de la actual.", TipoError.Validacion);
    public static readonly Error CuentaYaVerificada =
        new("auth.cuenta_ya_verificada", "Esta cuenta ya está verificada.", TipoError.Conflicto);
}
