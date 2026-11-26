using System.Text.RegularExpressions;

namespace Norvia.Application.Features.AccesoRecurso;

public static partial class RutaNormalizada
{
    /// <summary>"api/riesgos/{Id:guid}" → "api/riesgos/{id}". Con esta misma función se guarda Permiso.RutaPatron.</summary>
    public static string De(string? patron) =>
        Parametro().Replace((patron ?? "").Trim('/'), "{$1}").ToLowerInvariant();

    [GeneratedRegex(@"\{(\w+)(?::[^}]*)?\}")]
    private static partial Regex Parametro();
}
