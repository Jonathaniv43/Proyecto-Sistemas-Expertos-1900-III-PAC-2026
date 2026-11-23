using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Norvia.Infrastructure.Opciones;

namespace Norvia.API.Extensiones;

public static class AutenticacionExtensiones
{
    /// <summary>
    /// Valida JWT entrantes. Prioridad: header Bearer; si falta, cookie de access (Cookies__AccessName).
    /// </summary>
    public static IServiceCollection AddAutenticacionJwt(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOpciones>, IOptions<CookiesOpciones>>((bearer, jwt, cookies) =>
            {
                var o = jwt.Value;
                var cookieAccess = cookies.Value.AccessName;

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

                bearer.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var auth = ctx.Request.Headers.Authorization.FirstOrDefault();
                        if (!string.IsNullOrEmpty(auth) &&
                            auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Token = auth["Bearer ".Length..].Trim();
                        }
                        else if (ctx.Request.Cookies.TryGetValue(cookieAccess, out var desdeCookie) &&
                                 !string.IsNullOrWhiteSpace(desdeCookie))
                        {
                            ctx.Token = desdeCookie;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
