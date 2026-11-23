namespace Norvia.Infrastructure.Opciones;

/// <summary>
/// Sección <c>Cors</c> (variable <c>Cors__Origenes</c> del .env): orígenes del frontend web que
/// pueden llamar a la API desde el navegador, separados por coma y sin barra final.
/// Ejemplo: <c>https://norvia.vercel.app,http://localhost:5173</c>.
/// </summary>
public sealed class CorsOpciones
{
    public const string Seccion = "Cors";

    public string Origenes { get; set; } = string.Empty;

    /// <summary>Lista de orígenes ya separada y sin espacios. Único lugar donde se interpreta.</summary>
    public string[] Lista() =>
        Origenes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Un origen válido es solo esquema + host (+ puerto): <c>https://app.ejemplo.com</c>.</summary>
    public bool SonValidos() =>
        Lista() is { Length: > 0 } lista && lista.All(origen =>
            Uri.TryCreate(origen, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && uri.AbsolutePath == "/" && !origen.EndsWith('/')
            && string.IsNullOrEmpty(uri.Query));
}
