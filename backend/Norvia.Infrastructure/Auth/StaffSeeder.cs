using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Persistence;

namespace Norvia.Infrastructure.Auth;

public class StaffSeeder(
    IServiceProvider servicios,
    IConfiguration configuration,
    ILogger<StaffSeeder> log) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NorviaDbContext>();

        if (!await db.Database.CanConnectAsync(cancellationToken))
            return;

        try
        {
            await SembrarAsync(db, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "StaffSeeder omitido (¿falta migración?). Ejecutá dotnet ef database update.");
        }
    }

    private async Task SembrarAsync(NorviaDbContext db, CancellationToken cancellationToken)
    {
        var email = configuration["StaffSeed__Email"] ?? configuration["StaffSeed:Email"];
        var password = configuration["StaffSeed__Password"] ?? configuration["StaffSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Faltan StaffSeed__Email y/o StaffSeed__Password. Definilos en .env (ver example.env en Norvia.API); no hay valores por defecto en código.");
        }

        email = email.Trim().ToLowerInvariant();

        if (!await db.RolStaffPlataforma.AnyAsync(r => r.Id == 1, cancellationToken))
        {
            db.RolStaffPlataforma.Add(new RolStaffPlataforma
            {
                Id = 1,
                Codigo = "admin",
                Nombre = "Administrador plataforma"
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        if (await db.Acceso.AnyAsync(a => a.Email == email, cancellationToken))
            return;
        var hasher = new PasswordHasher<Acceso>();
        var acceso = new Acceso
        {
            Email = email,
            Verificado = true
        };
        acceso.PasswordHash = hasher.HashPassword(acceso, password);
        db.Acceso.Add(acceso);
        db.StaffPlataforma.Add(new StaffPlataforma
        {
            Acceso = acceso,
            RolStaffPlataformaId = 1,
            Activo = true
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
