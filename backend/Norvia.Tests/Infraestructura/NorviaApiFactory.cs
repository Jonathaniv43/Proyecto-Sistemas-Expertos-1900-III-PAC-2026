using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Norvia.Application.Common.Correo;
using Microsoft.EntityFrameworkCore;
using Norvia.Infrastructure.Data;
using Norvia.Infrastructure.Persistence.Interceptores;
using Testcontainers.PostgreSql;

namespace Norvia.Tests.Infraestructura;

/// <summary>
/// Levanta la API real en memoria sobre un PostgreSQL desechable (Testcontainers). No usa el .env ni la
/// base de desarrollo: cada ejecución crea su contenedor, aplica las migraciones y lo destruye al final.
/// Requiere Docker.
/// </summary>
public sealed class NorviaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string StaffEmail = "staff@pruebas.norvia";
    public const string StaffPassword = "Staff#Pruebas2026";
    public const string OrigenFrontend = "http://localhost:5173";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public string CadenaConexion => _postgres.GetConnectionString();

    /// <summary>Correos que la API "envió" durante las pruebas (no se usa SMTP).</summary>
    public CorreosCapturados Correos { get; } = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // El esquema se crea antes de arrancar la API, porque los seeders corren al iniciar.
        await using var db = NuevoContexto(empresaId: null);
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:PostgresConnection", CadenaConexion);
        builder.UseSetting("Jwt:Key", Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)));
        builder.UseSetting("Jwt:Issuer", "norvia-pruebas");
        builder.UseSetting("Jwt:Audience", "norvia-pruebas");
        builder.UseSetting("Jwt:AccessMinutos", "15");
        builder.UseSetting("Jwt:RefreshDias", "1");
        builder.UseSetting("EmpresaId:Key", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        builder.UseSetting("Cors:Origenes", OrigenFrontend);
        builder.UseSetting("StaffSeed:Email", StaffEmail);
        builder.UseSetting("StaffSeed:Password", StaffPassword);
        builder.ConfigureTestServices(servicios => servicios.AddSingleton<IEnvioCorreo>(Correos));
    }

    /// <summary>
    /// DbContext fuera de la API, con la empresa de sesión que se indique (nula = sin sesión de empresa).
    /// Incluye el interceptor que asigna y protege EmpresaId, igual que en la API.
    /// </summary>
    public NorviaDbContext NuevoContexto(Guid? empresaId)
    {
        var tenant = new TenantFijo(empresaId);
        var opciones = new DbContextOptionsBuilder<NorviaDbContext>()
            .UseNpgsql(CadenaConexion)
            .AddInterceptors(new AsignarEmpresaInterceptor(tenant))
            .Options;
        return new NorviaDbContext(opciones, tenant);
    }
}

/// <summary>Todas las clases de prueba comparten un solo contenedor y una sola API.</summary>
[CollectionDefinition(Nombre)]
public sealed class ColeccionApi : ICollectionFixture<NorviaApiFactory>
{
    public const string Nombre = "api";
}
