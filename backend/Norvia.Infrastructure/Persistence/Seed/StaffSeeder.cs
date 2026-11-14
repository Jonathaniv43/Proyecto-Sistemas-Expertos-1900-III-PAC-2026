using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>Crea el primer staff de plataforma con las credenciales del .env (StaffSeed__*).</summary>
public sealed class StaffSeeder(
    NorviaDbContext db,
    IHasherPassword hasher,
    IOptions<StaffSeedOpciones> opciones,
    ILogger<StaffSeeder> log)
{
    public async Task EjecutarAsync(CancellationToken cancellationToken)
    {
        if (!await db.Database.CanConnectAsync(cancellationToken))
            return;

        var seed = opciones.Value;
        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.Password))
            throw new InvalidOperationException(
                "Faltan StaffSeed__Email y/o StaffSeed__Password. Definilos en .env (ver example.env en Norvia.API); no hay valores por defecto en código.");

        try
        {
            await SembrarAsync(seed.Email.Trim().ToLowerInvariant(), seed.Password, cancellationToken);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            log.LogWarning(ex, "StaffSeeder omitido (¿falta migración?). Ejecutá dotnet ef database update.");
        }
    }

    private async Task SembrarAsync(string email, string password, CancellationToken cancellationToken)
    {
        if (await db.Acceso.AnyAsync(a => a.Email == email, cancellationToken))
            return;

        var acceso = new Acceso { Email = email, Verificado = true };
        acceso.PasswordHash = hasher.Hashear(acceso, password);
        db.Acceso.Add(acceso);
        // El rol admin_plataforma lo crea la migración (CatalogoBase, HU-097).
        db.StaffPlataforma.Add(new StaffPlataforma { Acceso = acceso, RolStaffPlataformaId = CatalogoBase.RolStaffAdminId, Activo = true });
        await db.SaveChangesAsync(cancellationToken);
    }
}
