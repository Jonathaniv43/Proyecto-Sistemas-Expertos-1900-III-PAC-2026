using System.Security.Cryptography;

namespace Norvia.Application.Common.Seguridad;

/// <summary>
/// Genera contraseñas temporales que cumplen la política de HU-114 (mayúscula, minúscula, número y
/// carácter especial). Usa caracteres sin ambigüedad visual (sin 0/O, 1/l/I) porque el administrador
/// las entrega a mano mientras no exista el envío por correo (HU-118).
/// </summary>
public static class GeneradorContrasenaTemporal
{
    private const string Mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Minusculas = "abcdefghijkmnopqrstuvwxyz";
    private const string Numeros = "23456789";
    private const string Especiales = "#$%&*+-=?@";
    private const int Largo = 12;

    public static string Generar()
    {
        Span<char> c = stackalloc char[Largo];
        c[0] = Uno(Mayusculas);
        c[1] = Uno(Minusculas);
        c[2] = Uno(Numeros);
        c[3] = Uno(Especiales);
        const string todos = Mayusculas + Minusculas + Numeros + Especiales;
        for (var i = 4; i < Largo; i++) c[i] = Uno(todos);

        // Mezcla para que los requisitos no queden siempre al inicio.
        for (var i = Largo - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (c[i], c[j]) = (c[j], c[i]);
        }
        return new string(c);
    }

    private static char Uno(string conjunto) => conjunto[RandomNumberGenerator.GetInt32(conjunto.Length)];
}
