namespace Norvia.Application.Features.Catalogos;

internal static class Texto
{
    /// <summary>Recorta el texto y convierte el vacío en nulo (columnas opcionales).</summary>
    public static string? Opcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
