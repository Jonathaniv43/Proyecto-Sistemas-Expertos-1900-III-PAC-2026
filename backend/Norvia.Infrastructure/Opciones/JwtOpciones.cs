using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Norvia.Infrastructure.Opciones;

/// <summary>Sección <c>Jwt</c> (variables <c>Jwt__*</c> del .env).</summary>
public sealed class JwtOpciones
{
    public const string Seccion = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "norvia";
    public string Audience { get; set; } = "norvia";
    public int AccessMinutos { get; set; } = 15;
    public int RefreshDias { get; set; } = 7;

    /// <summary>Clave de firma derivada de <see cref="Key"/>. Único lugar donde se calcula.</summary>
    public SymmetricSecurityKey ClaveFirma() => new(SHA256.HashData(Encoding.UTF8.GetBytes(Key)));
}
