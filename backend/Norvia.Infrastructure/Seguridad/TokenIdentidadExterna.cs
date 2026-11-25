using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Seguridad;

/// <summary>
/// <c>base64url(json).base64url(HMAC-SHA256)</c>. La clave se deriva de la del JWT con un prefijo propio,
/// para que una firma de otro uso (sesión, cambio de contraseña) no sirva aquí.
/// </summary>
public sealed class TokenIdentidadExterna(IOptions<JwtOpciones> opciones) : ITokenIdentidadExterna
{
    private readonly byte[] _clave = SHA256.HashData(Encoding.UTF8.GetBytes("norvia:identidad-externa:" + opciones.Value.Key));

    private sealed record Carga(string P, string U, string E, string N1, string? N2, string A1, string? A2, long X, bool V);

    public string Crear(IdentidadExterna i, DateTime expiraUtc)
    {
        var carga = new Carga(
            i.Proveedor.ToString(), i.ProveedorUserId, i.Email, i.PrimerNombre, i.SegundoNombre,
            i.PrimerApellido, i.SegundoApellido, new DateTimeOffset(expiraUtc, TimeSpan.Zero).ToUnixTimeSeconds(), i.CorreoVerificado);
        var datos = JsonSerializer.SerializeToUtf8Bytes(carga);
        return Base64Url(datos) + "." + Base64Url(HMACSHA256.HashData(_clave, datos));
    }

    public IdentidadExterna? Leer(string token, DateTime ahoraUtc)
    {
        var partes = token.Split('.');
        if (partes.Length != 2) return null;

        byte[] datos, firma;
        try
        {
            datos = DeBase64Url(partes[0]);
            firma = DeBase64Url(partes[1]);
        }
        catch (FormatException)
        {
            return null;
        }

        if (!CryptographicOperations.FixedTimeEquals(HMACSHA256.HashData(_clave, datos), firma)) return null;

        Carga? carga;
        try
        {
            carga = JsonSerializer.Deserialize<Carga>(datos);
        }
        catch (JsonException)
        {
            return null;
        }

        if (carga is null
            || DateTimeOffset.FromUnixTimeSeconds(carga.X).UtcDateTime <= ahoraUtc
            || !Enum.TryParse<ProveedorIdentidad>(carga.P, out var proveedor))
            return null;

        return new IdentidadExterna(proveedor, carga.U, carga.E, carga.N1, carga.N2, carga.A1, carga.A2, carga.V);
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] DeBase64Url(string s) =>
        Convert.FromBase64String(s.Replace('-', '+').Replace('_', '/').PadRight((s.Length + 3) / 4 * 4, '='));
}
