using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Data;

/// <summary>
/// Fábrica de diseño (convención de EF Core) usada solo por <c>dotnet ef</c> (migrations add,
/// database update, migrations bundle). No participa en el arranque real de la app: eso sigue
/// pasando por <c>Program.cs</c>, con todo el DI y las validaciones normales.
/// <para>
/// Evita que <c>dotnet ef</c> tenga que levantar el host completo (y sus <c>ValidateOnStart()</c> de
/// Jwt, EmpresaId, Cors, etc.) solo para encontrar el tipo de <see cref="NorviaDbContext"/>: aquí solo
/// se necesita la cadena de conexión.
/// </para>
/// </summary>
public sealed class NorviaDbContextFactory : IDesignTimeDbContextFactory<NorviaDbContext>
{
    public NorviaDbContext CreateDbContext(string[] args)
    {
        // dotnet ef no pasa por Program.cs (que carga el .env con Env.Load()), así que en local
        // hay que cargarlo aquí. Cubre los dos directorios desde los que se corre el comando
        // (backend/ o backend/Norvia.API/). En despliegue no hay .env y la variable ya está en el
        // entorno, así que no hace nada.
        var rutaEnv = new[] { ".env", Path.Combine("Norvia.API", ".env") }.FirstOrDefault(File.Exists);
        if (rutaEnv is not null)
            Env.Load(rutaEnv);

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")
            ?? throw new InvalidOperationException("Falta ConnectionStrings__PostgresConnection en el entorno.");

        var options = new DbContextOptionsBuilder<NorviaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NorviaDbContext(options, new SinSesionTenantActual());
    }

    /// <summary>Sin sesión de empresa (igual que login/seeders): correcto para migraciones, que no filtran por empresa.</summary>
    private sealed class SinSesionTenantActual : ITenantActual
    {
        public Guid? EmpresaId => null;
        public Guid? VinculoEmpresaId => null;
        public Guid? AccesoId => null;
        public TipoSesion? TipoSesion => null;
    }
}
