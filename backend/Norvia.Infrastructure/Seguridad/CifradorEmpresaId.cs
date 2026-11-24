using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Interfaces;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Seguridad;

/// <summary>AES-GCM sobre el Guid de la empresa. Salida: base64url(nonce | tag | cifrado).</summary>
public sealed class CifradorEmpresaId(IOptions<CifradoOpciones> opciones) : ICifradorEmpresaId
{
    private readonly byte[] _clave = SHA256.HashData(Encoding.UTF8.GetBytes(opciones.Value.Key));

    public string Cifrar(Guid empresaId)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plano = empresaId.ToByteArray();
        var cipher = new byte[plano.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(_clave, 16);
        aes.Encrypt(nonce, plano, cipher, tag);
        return ABase64Url([.. nonce, .. tag, .. cipher]);
    }

    public bool TryDescifrar(string valor, out Guid empresaId)
    {
        empresaId = Guid.Empty;
        try
        {
            var raw = DeBase64Url(valor);
            if (raw.Length != 12 + 16 + 16)
                return false;
            var plano = new byte[16];
            using var aes = new AesGcm(_clave, 16);
            aes.Decrypt(raw[..12], raw[28..], raw[12..28], plano);
            empresaId = new Guid(plano);
            return true;
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException)
        {
            return false;
        }
    }

    /// <summary>base64url sin relleno (A-Z a-z 0-9 - _): viaja en la URL del login sin %-escapes.</summary>
    private static string ABase64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    /// <summary>Acepta base64url y también base64 estándar (ids y JWT emitidos antes del cambio).</summary>
    private static byte[] DeBase64Url(string valor)
    {
        var base64 = valor.Trim().Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64.PadRight((base64.Length + 3) / 4 * 4, '='));
    }
}
