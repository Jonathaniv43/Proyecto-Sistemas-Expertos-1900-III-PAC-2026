using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Interfaces;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Seguridad;

/// <summary>
/// HMAC-SHA256 sobre (AccesoId | vencimiento | sello). Salida en base64url:
/// 16 bytes de Guid + 8 de vencimiento (segundos Unix) + 16 de sello + 32 de firma.
/// La clave se deriva de la del JWT con un prefijo propio, para que una firma no sirva en el otro uso.
/// </summary>
public sealed class TokenCambioContrasena(IOptions<JwtOpciones> opciones) : ITokenCambioContrasena
{
    private const int LargoDatos = 16 + 8 + 16;
    private const int LargoFirma = 32;

    private readonly byte[] _clave = SHA256.HashData(Encoding.UTF8.GetBytes("norvia:cambio-contrasena:" + opciones.Value.Key));

    public string Crear(Guid accesoId, string? passwordHashActual, DateTime expiraUtc)
    {
        var datos = new byte[LargoDatos];
        accesoId.TryWriteBytes(datos.AsSpan(0, 16));
        BinaryPrimitives.WriteInt64BigEndian(datos.AsSpan(16, 8), new DateTimeOffset(expiraUtc, TimeSpan.Zero).ToUnixTimeSeconds());
        Sello(accesoId, passwordHashActual).CopyTo(datos.AsSpan(24, 16));
        return Base64Url([.. datos, .. HMACSHA256.HashData(_clave, datos)]);
    }

    public EnlaceCambioContrasena? Leer(string token)
    {
        byte[] raw;
        try
        {
            raw = Convert.FromBase64String(token.Replace('-', '+').Replace('_', '/').PadRight((token.Length + 3) / 4 * 4, '='));
        }
        catch (FormatException)
        {
            return null;
        }
        if (raw.Length != LargoDatos + LargoFirma) return null;

        var datos = raw.AsSpan(0, LargoDatos);
        if (!CryptographicOperations.FixedTimeEquals(HMACSHA256.HashData(_clave, datos), raw.AsSpan(LargoDatos)))
            return null;

        var segundos = BinaryPrimitives.ReadInt64BigEndian(datos.Slice(16, 8));
        return new EnlaceCambioContrasena(
            new Guid(datos[..16]),
            DateTimeOffset.FromUnixTimeSeconds(segundos).UtcDateTime,
            datos.Slice(24, 16).ToArray());
    }

    public bool SelloVigente(EnlaceCambioContrasena enlace, string? passwordHashActual) =>
        CryptographicOperations.FixedTimeEquals(enlace.Sello, Sello(enlace.AccesoId, passwordHashActual));

    // Sin contraseña, el sello sale del Id: al agregar la contraseña cambia y el enlace queda usado. No es una contraseña.
    private static byte[] Sello(Guid accesoId, string? passwordHash) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash ?? $"sin-contrasena:{accesoId:N}"))[..16];

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
