using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Norvia.Application.Features.Auth.Interfaces;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Auth;

public class JwtTokenServicio : ITokenServicio
{
    private readonly ICifradorEmpresaId _cifrador;
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessMinutos;
    private readonly int _refreshDias;

    public JwtTokenServicio(IConfiguration configuration, ICifradorEmpresaId cifrador)
    {
        _cifrador = cifrador;
        var secreto = configuration["Jwt__Key"] ?? configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta Jwt__Key.");
        _issuer = configuration["Jwt__Issuer"] ?? configuration["Jwt:Issuer"] ?? "norvia";
        _audience = configuration["Jwt__Audience"] ?? configuration["Jwt:Audience"] ?? "norvia";
        _accessMinutos = int.TryParse(configuration["Jwt__AccessMinutos"] ?? configuration["Jwt:AccessMinutos"], out var m) ? m : 15;
        _refreshDias = int.TryParse(configuration["Jwt__RefreshDias"] ?? configuration["Jwt:RefreshDias"], out var d) ? d : 7;
        _key = new SymmetricSecurityKey(SHA256.HashData(Encoding.UTF8.GetBytes(secreto)));
    }

    public int AccessSegundos => _accessMinutos * 60;
    public TimeSpan RefreshDuracion => TimeSpan.FromDays(_refreshDias);

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
            claims.Add(new Claim("empresa_id", _cifrador.Cifrar(empresa)));

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: ahoraUtc,
            expires: ahoraUtc.AddMinutes(_accessMinutos),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public (string TokenPlano, string Hash) CrearRefresh()
    {
        var plano = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return (plano, HashRefresh(plano));
    }

    public string HashRefresh(string tokenPlano)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(tokenPlano));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
