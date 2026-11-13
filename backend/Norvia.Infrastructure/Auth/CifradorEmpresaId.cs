using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Norvia.Application.Features.Auth.Interfaces;

namespace Norvia.Infrastructure.Auth;

public class CifradorEmpresaId : ICifradorEmpresaId
{
    private readonly byte[] _clave;

    public CifradorEmpresaId(IConfiguration configuration)
    {
        var secreto = configuration["EmpresaId__Key"]
            ?? configuration["EmpresaId:Key"]
            ?? throw new InvalidOperationException("Falta EmpresaId__Key en el entorno.");
        _clave = SHA256.HashData(Encoding.UTF8.GetBytes(secreto));
    }

    public string Cifrar(Guid empresaId)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plano = empresaId.ToByteArray();
        var cipher = new byte[plano.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(_clave, 16);
        aes.Encrypt(nonce, plano, cipher, tag);
        return Convert.ToBase64String(nonce.Concat(tag).Concat(cipher).ToArray());
    }

    public bool TryDescifrar(string valor, out Guid empresaId)
    {
        empresaId = Guid.Empty;
        try
        {
            var raw = Convert.FromBase64String(valor);
            if (raw.Length != 12 + 16 + 16)
                return false;
            var nonce = raw[..12];
            var tag = raw[12..28];
            var cipher = raw[28..];
            var plano = new byte[16];
            using var aes = new AesGcm(_clave, 16);
            aes.Decrypt(nonce, cipher, tag, plano);
            empresaId = new Guid(plano);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
