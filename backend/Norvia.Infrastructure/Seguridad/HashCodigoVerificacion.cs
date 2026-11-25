using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Interfaces;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Seguridad;

/// <summary>SHA-256 en hexadecimal, con la clave del JWT como sal fija de este uso.</summary>
public sealed class HashCodigoVerificacion(IOptions<JwtOpciones> opciones) : IHashCodigoVerificacion
{
    private readonly string _sal = "norvia:codigo-correo:" + opciones.Value.Key;

    public string Generar() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    public string Hashear(string codigo) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(_sal + codigo.Trim())));

    public bool Coincide(string hash, string codigo) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(hash), Encoding.UTF8.GetBytes(Hashear(codigo)));
}
