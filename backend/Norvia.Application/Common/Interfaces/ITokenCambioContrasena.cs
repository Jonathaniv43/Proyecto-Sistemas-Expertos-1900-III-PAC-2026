namespace Norvia.Application.Common.Interfaces;

/// <summary>Contenido de un enlace de cambio de contraseña ya verificado (firma correcta).</summary>
public sealed record EnlaceCambioContrasena(Guid AccesoId, DateTime ExpiraUtc, byte[] Sello);

/// <summary>
/// Token del enlace de cambio de contraseña (HU-022). Va firmado y no se guarda en la base: lleva el
/// Acceso, el vencimiento y un sello de la contraseña actual. Cuando la contraseña cambia, el sello
/// deja de coincidir, así que cada enlace sirve una sola vez.
/// </summary>
public interface ITokenCambioContrasena
{
    /// <param name="passwordHashActual">Nulo si la cuenta todavía no tiene contraseña (creada con Microsoft): el enlace sirve para agregarla.</param>
    string Crear(Guid accesoId, string? passwordHashActual, DateTime expiraUtc);

    /// <summary>null si el token está mal formado o la firma no es válida.</summary>
    EnlaceCambioContrasena? Leer(string token);

    /// <summary>true si el sello corresponde a la contraseña que el Acceso tiene ahora.</summary>
    bool SelloVigente(EnlaceCambioContrasena enlace, string? passwordHashActual);
}
