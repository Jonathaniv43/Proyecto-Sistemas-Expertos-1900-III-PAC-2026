using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Norvia.Infrastructure.Opciones;

namespace Norvia.API.Extensiones;

public static class CorsExtensiones
{
    /// <summary>
    /// Política CORS por defecto: solo los orígenes de <see cref="CorsOpciones"/>.
    /// Sin credenciales (cookies): el JWT viaja en el encabezado Authorization.
    /// </summary>
    public static IServiceCollection AddCorsFrontend(this IServiceCollection services)
    {
        services.AddCors();
        services.AddOptions<CorsOptions>()
            .Configure<IOptions<CorsOpciones>>((cors, opciones) =>
                cors.AddDefaultPolicy(politica => politica
                    .WithOrigins(opciones.Value.Lista())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));
        return services;
    }
}
