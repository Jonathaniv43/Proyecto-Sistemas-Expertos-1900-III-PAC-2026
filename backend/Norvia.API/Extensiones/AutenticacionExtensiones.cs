using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Norvia.Infrastructure.Opciones;

namespace Norvia.API.Extensiones;

public static class AutenticacionExtensiones
{
    /// <summary>Valida los JWT entrantes con las mismas <see cref="JwtOpciones"/> con que se emiten.</summary>
    public static IServiceCollection AddAutenticacionJwt(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOpciones>>((bearer, jwt) =>
            {
                var o = jwt.Value;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = o.Issuer,
                    ValidAudience = o.Audience,
                    IssuerSigningKey = o.ClaveFirma(),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        return services;
    }
}
