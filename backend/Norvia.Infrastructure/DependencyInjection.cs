using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        return services;
    }
}
