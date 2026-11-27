namespace Norvia.Domain.Enums;

/// <summary>Para qué se pide el código de 6 dígitos de <see cref="Norvia.Domain.Entities.Acceso.VerificacionCorreo"/>.</summary>
public enum PropositoVerificacionCorreo
{
    /// <summary>D29, RN-86: confirma el correo de un proveedor de identidad antes de crear la cuenta.</summary>
    Microsoft,

    /// <summary>Activa una cuenta nueva creada por otra persona (registro de empresa, alta de vínculo o de encuestado).</summary>
    ActivacionCuenta
}
