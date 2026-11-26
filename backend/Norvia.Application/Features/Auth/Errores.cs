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

    /// <summary>D29: el token de identidad no es válido, venció o el proveedor rechazó el código.</summary>
    public static readonly Error IdentidadInvalida =
        new("auth.identidad_invalida", "No se pudo validar la cuenta de Microsoft. Intenta de nuevo.", TipoError.NoAutorizado);

    public static readonly Error CodigoIncorrecto =
        new("auth.codigo_incorrecto", "El código no es correcto o ya venció. Pide uno nuevo.", TipoError.Validacion);

    public static readonly Error DemasiadosIntentos =
        new("auth.demasiados_intentos", "Hiciste demasiados intentos. Espera unos minutos e intenta de nuevo.", TipoError.DemasiadasSolicitudes);

    /// <summary>RN-86: crear una cuenta con un proveedor exige haber confirmado el correo con el código.</summary>
    public static readonly Error CorreoSinVerificar =
        new("auth.correo_sin_verificar", "Confirma tu correo con el código que te enviamos antes de registrar la empresa.", TipoError.Prohibido);

    /// <summary>El correo ya tiene cuenta (o esa cuenta del proveedor ya está vinculada): no hay nada que confirmar.</summary>
    public static readonly Error CodigoNoRequerido =
        new("auth.codigo_no_requerido", "Este correo ya tiene una cuenta en Norvia.", TipoError.Conflicto);

    public static readonly Error CorreoNoEnviado =
        new("auth.correo_no_enviado", "No pudimos enviar el código. Intenta de nuevo en unos minutos.", TipoError.Servidor);

    /// <summary>RN-82: esa cuenta del proveedor ya pertenece a otro Acceso.</summary>
    public static readonly Error ProveedorYaVinculado =
        new("auth.proveedor_ya_vinculado", "Esa cuenta de Microsoft ya está vinculada a otra cuenta de Norvia.", TipoError.Conflicto);
}
