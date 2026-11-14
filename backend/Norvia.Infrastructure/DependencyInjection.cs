using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Application.Common.Interfaces;
using Norvia.Infrastructure.Opciones;
using Norvia.Infrastructure.Persistence;
using Norvia.Infrastructure.Persistence.Seed;
using Norvia.Infrastructure.Seguridad;
using Norvia.Infrastructure.Tiempo;

namespace Norvia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'PostgresConnection'. Copiá example.env a .env en Norvia.API.");

        services.AddDbContext<NorviaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        // Opciones: la configuración se lee solo aquí (patrón Options).
        services.AddOptions<JwtOpciones>()
            .Bind(configuration.GetSection(JwtOpciones.Seccion))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Falta Jwt__Key.")
            .Validate(o => o.AccessMinutos > 0 && o.RefreshDias > 0, "Jwt__AccessMinutos y Jwt__RefreshDias deben ser > 0.")
            .ValidateOnStart();
        services.AddOptions<CifradoOpciones>()
            .Bind(configuration.GetSection(CifradoOpciones.Seccion))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Falta EmpresaId__Key en el entorno.")
            .ValidateOnStart();
        services.AddOptions<StaffSeedOpciones>()
            .Bind(configuration.GetSection(StaffSeedOpciones.Seccion));

        // Servicios transversales.
        services.AddScoped<IReloj, RelojBaseDatos>();
        services.AddSingleton<ICifradorEmpresaId, CifradorEmpresaId>();
        services.AddSingleton<ITokenServicio, JwtTokenServicio>();
        services.AddSingleton<IHasherPassword, HasherPassword>();

        // Repositorios por slice: I<CasoDeUso>Repositorio → <CasoDeUso>Repositorio.
        var repositorios = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Repositorio"));
        foreach (var implementacion in repositorios)
        {
            var contrato = implementacion.GetInterfaces().SingleOrDefault(i => i.Name == "I" + implementacion.Name);
            if (contrato is not null)
                services.AddScoped(contrato, implementacion);
        }

        services.AddScoped<StaffSeeder>();

        return services;
    }
}
