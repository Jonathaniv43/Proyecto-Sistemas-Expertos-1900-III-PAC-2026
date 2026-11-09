using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Norvia.Infrastructure.Persistence.Conversiones;

/// <summary>
/// Guarda un enum como texto en minúsculas snake_case (BajaLaboral → "baja_laboral"),
/// como pide el modelo de datos §1 "Valores con opciones fijas".
/// </summary>
public sealed class SnakeCaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
    v => ATexto(v),
    s => DeTexto(s))
    where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> PorValor =
        Enum.GetValues<TEnum>().ToDictionary(v => v, v => ASnakeCase(v.ToString()));

    private static readonly Dictionary<string, TEnum> PorTexto =
        PorValor.ToDictionary(p => p.Value, p => p.Key);

    /// <summary>Todos los valores permitidos, en el orden del enum. Se usan para el CHECK.</summary>
    public static IReadOnlyCollection<string> Valores => PorValor.Values;

    public static string ATexto(TEnum valor) => PorValor[valor];

    public static TEnum DeTexto(string texto) =>
        PorTexto.TryGetValue(texto, out var valor)
            ? valor
            : throw new InvalidOperationException($"'{texto}' no es un valor válido de {typeof(TEnum).Name}.");

    private static string ASnakeCase(string nombre)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < nombre.Length; i++)
        {
            if (i > 0 && char.IsUpper(nombre[i])) sb.Append('_');
            sb.Append(char.ToLowerInvariant(nombre[i]));
        }
        return sb.ToString();
    }
}
