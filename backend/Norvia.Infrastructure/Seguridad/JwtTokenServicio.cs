using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Seguridad;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Seguridad;

public sealed class JwtTokenServicio(IOptions<JwtOpciones> opciones, ICifradorEmpresaId cifrador) : ITokenServicio
{
    private readonly JwtOpciones _opciones = opciones.Value;
    private readonly SigningCredentials _credenciales =
        new(opciones.Value.ClaveFirma(), SecurityAlgorithms.HmacSha256);

    public int AccessSegundos => _opciones.AccessMinutos * 60;
    public TimeSpan RefreshDuracion => TimeSpan.FromDays(_opciones.RefreshDias);

    public string CrearAccess(SesionToken sesion, DateTime ahoraUtc)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, sesion.AccesoId.ToString()),
            new("tipo_sesion", sesion.Tipo == TipoSesion.Empresa ? "empresa" : "plataforma"),
        };
        if (sesion.RolCodigo is not null)
            claims.Add(new Claim(ClaimTypes.Role, sesion.RolCodigo));
        if (sesion.VinculoEmpresaId is Guid vinculo)
            claims.Add(new Claim("vinculo_id", vinculo.ToString()));
        if (sesion.EmpresaId is Guid empresa)
            claims.Add(new Claim("empresa_id", cifrador.Cifrar(empresa)));

        var jwt = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: ahoraUtc,
            expires: ahoraUtc.AddMinutes(_opciones.AccessMinutos),
            signingCredentials: _credenciales);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public (string TokenPlano, string Hash) CrearRefresh()
    {
        var plano = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return (plano, HashRefresh(plano));
    }

    public string HashRefresh(string tokenPlano) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenPlano))).ToLowerInvariant();
}
