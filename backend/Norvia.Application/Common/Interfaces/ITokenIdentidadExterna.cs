namespace Norvia.Application.Common.Interfaces;

/// <summary>
/// Token propio de Norvia, firmado y de corta vida, que acompaña los pasos posteriores a la identidad
/// validada con un proveedor (registro, login y vinculación). El cliente nunca envía el correo ni el
/// identificador del proveedor como dato de confianza (RN-84).
/// </summary>
public interface ITokenIdentidadExterna
{
    string Crear(IdentidadExterna identidad, DateTime expiraUtc);

    /// <summary>null si el token está mal formado, la firma no es válida o ya venció.</summary>
    IdentidadExterna? Leer(string token, DateTime ahoraUtc);
}
