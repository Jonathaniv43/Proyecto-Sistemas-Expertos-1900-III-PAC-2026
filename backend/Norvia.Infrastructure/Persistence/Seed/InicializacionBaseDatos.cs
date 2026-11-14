using Microsoft.Extensions.DependencyInjection;

namespace Norvia.Infrastructure.Persistence.Seed;

public static class InicializacionBaseDatos
{
    /// <summary>Ejecuta los seeders al arrancar. Se llama desde Program.cs.</summary>
    public static async Task InicializarBaseDatosAsync(this IServiceProvider servicios, CancellationToken cancellationToken = default)
    {
        using var scope = servicios.CreateScope();
        await scope.ServiceProvider.GetRequiredService<StaffSeeder>().EjecutarAsync(cancellationToken);
    }
}
