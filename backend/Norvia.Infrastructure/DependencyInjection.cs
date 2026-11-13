using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Norvia.Application.Features.Auth.Interfaces;
using Norvia.Infrastructure.Auth;
using Norvia.Infrastructure.Persistence;

namespace Norvia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'PostgresConnection'. Copiá example.env a .env en Norvia.API.");

        services.AddDbContext<NorviaDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IRelojBaseDatos, RelojBaseDatos>();
        services.AddSingleton<ICifradorEmpresaId, CifradorEmpresaId>();
        services.AddSingleton<ITokenServicio, JwtTokenServicio>();
        services.AddScoped<IAuthRepositorio, AuthRepositorio>();
        services.AddScoped<IVerificadorPassword, VerificadorPassword>();
        services.AddHostedService<StaffSeeder>();

        var jwtSecreto = configuration["Jwt__Key"] ?? configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta Jwt__Key.");
        var issuer = configuration["Jwt__Issuer"] ?? configuration["Jwt:Issuer"] ?? "norvia";
        var audience = configuration["Jwt__Audience"] ?? configuration["Jwt:Audience"] ?? "norvia";
        var key = new SymmetricSecurityKey(SHA256.HashData(Encoding.UTF8.GetBytes(jwtSecreto)));

        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        return services;
    }
}
