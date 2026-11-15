// Norvia.Application/Common/Seguridad/RutaNormalizada.cs
using System.Text.RegularExpressions;

namespace Norvia.Application.Common.Seguridad;

public static partial class RutaNormalizada
{
    /// "api/riesgos/{Id:guid}" → "api/riesgos/{id}". Con esta misma función se guarda Permiso.RutaPatron.
    public static string De(string? patron) =>
        Parametro().Replace((patron ?? "").Trim('/'), "{$1}").ToLowerInvariant();

    [GeneratedRegex(@"\{(\w+)(?::[^}]*)?\}")]
    private static partial Regex Parametro();
}