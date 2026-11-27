namespace Norvia.Application.Common.Correo;

/// <summary>Códigos estables de <see cref="Norvia.Domain.Entities.Catalogos.PlantillaCorreo"/>.</summary>
public static class CodigosPlantillaCorreo
{
    public const string CambioContrasena = "cambio_contrasena";

    /// <summary>Código de 6 dígitos al registrarse con un proveedor de identidad (D29, RN-86).</summary>
    public const string CodigoVerificacionCorreo = "codigo_verificacion_correo";

    /// <summary>
    /// Código de 6 dígitos para activar una cuenta local nueva (registro de empresa, alta de vínculo o de
    /// encuestado): mismo mecanismo que <see cref="CodigoVerificacionCorreo"/>, aplicado a una cuenta ya creada.
    /// </summary>
    public const string ActivacionCuenta = "activacion_cuenta";

    /// <summary>Correo de una notificación de M11 (HU-081, HU-116), enviado por la bandeja de salida (HU-118).</summary>
    public const string Notificacion = "notificacion";
}