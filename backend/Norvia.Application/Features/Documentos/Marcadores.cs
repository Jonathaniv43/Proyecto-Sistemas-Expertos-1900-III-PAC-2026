using System.Globalization;
using System.Text.RegularExpressions;

namespace Norvia.Application.Features.Documentos;

/// <summary>Datos del perfil de la empresa que llenan los marcadores de una plantilla (HU-070).</summary>
public sealed record DatosEmpresaDocumento(string RazonSocial, string Rtn, string? Direccion, string? ActividadEconomica, string? Rubro);

/// <summary>
/// Marcadores de las plantillas de documento (D13, RN-49; 12-modelo-datos § PlantillaDocumento): <c>{{RazonSocial}}</c>,
/// <c>{{RTN}}</c>, <c>{{Direccion}}</c>, <c>{{ActividadEconomica}}</c>, <c>{{Rubro}}</c> y <c>{{Fecha}}</c>.
/// <list type="bullet">
/// <item>Si al perfil le falta el dato, el marcador queda como <c>[Completar: dirección]</c> para corregirlo en el
/// borrador (HU-071).</item>
/// <item>Un marcador fuera de la lista queda tal cual.</item>
/// <item>Los nombres no distinguen mayúsculas: <c>{{rtn}}</c> también vale.</item>
/// </list>
/// </summary>
public static partial class Marcadores
{
    private static readonly CultureInfo Honduras = new("es-HN");

    /// <summary>Lo que falta en el borrador: el texto de cada <c>[Completar: …]</c>, sin repetir y en orden.</summary>
    public static IReadOnlyList<string> PorCompletar(string contenido) =>
        PorCompletarRegex().Matches(contenido).Select(m => m.Groups[1].Value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>Reemplaza los marcadores con los datos de la empresa y la fecha de generación (hora de Honduras).</summary>
    public static string Reemplazar(string contenido, DatosEmpresaDocumento datos, DateOnly fecha) =>
        MarcadorRegex().Replace(contenido, m => Valor(m.Groups[1].Value, datos, fecha) ?? m.Value);

    private static string? Valor(string nombre, DatosEmpresaDocumento datos, DateOnly fecha) => nombre.ToLowerInvariant() switch
    {
        "razonsocial" => datos.RazonSocial,
        "rtn" => datos.Rtn,
        "direccion" => Dato(datos.Direccion, "dirección"),
        "actividadeconomica" => Dato(datos.ActividadEconomica, "actividad económica"),
        "rubro" => Dato(datos.Rubro, "rubro"),
        "fecha" => fecha.ToString("d 'de' MMMM 'de' yyyy", Honduras),
        _ => null
    };

    private static string Dato(string? valor, string etiqueta) =>
        string.IsNullOrWhiteSpace(valor) ? $"[Completar: {etiqueta}]" : valor.Trim();

    [GeneratedRegex(@"\{\{\s*([A-Za-z]+)\s*\}\}")]
    private static partial Regex MarcadorRegex();

    [GeneratedRegex(@"\[Completar:\s*([^\]\r\n]+?)\s*\]")]
    private static partial Regex PorCompletarRegex();
}
