using System.Security.Cryptography;
using Norvia.Application.Common.Interfaces;

namespace Norvia.Infrastructure.Seguridad;

/// <summary>Contraseña aleatoria criptográfica para envío por correo (18 caracteres).</summary>
public sealed class GeneradorContrasenaTemporal : IGeneradorContrasenaTemporal
{
    private const int Longitud = 18;
    private const string Caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";

    public string Generar()
    {
        Span<byte> bytes = stackalloc byte[Longitud];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[Longitud];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = Caracteres[bytes[i] % Caracteres.Length];
        return new string(chars);
    }
}