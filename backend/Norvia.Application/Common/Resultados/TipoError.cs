namespace Norvia.Application.Common.Resultados;

/// <summary>Categoría de un fallo de negocio. La API la traduce a un código HTTP.</summary>
public enum TipoError
{
    Validacion,     // 400
    NoAutorizado,   // 401
    Prohibido,      // 403
    NoEncontrado,   // 404
    Conflicto       // 409
}
