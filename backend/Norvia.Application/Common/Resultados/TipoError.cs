namespace Norvia.Application.Common.Resultados;

/// <summary>Categoría de un fallo de negocio. La API la traduce a un código HTTP.</summary>
public enum TipoError
{
    Validacion,     // 400
    NoAutorizado,   // 401
    Prohibido,      // 403
    NoEncontrado,   // 404
    Conflicto,      // 409
    Expirado,       // 410: el recurso existió pero ya no sirve (p. ej. un enlace vencido o usado)
    DemasiadasSolicitudes, // 429: se superó el límite de intentos (p. ej. adivinar un código)
    Servidor        // 500
}
