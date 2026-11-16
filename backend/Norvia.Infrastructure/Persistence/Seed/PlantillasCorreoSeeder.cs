using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>Sincroniza <see cref="PlantillaCorreo"/> con <see cref="CatalogoPlantillasCorreo"/> (fuente en código).</summary>
public sealed class PlantillasCorreoSeeder(NorviaDbContext db, ILogger<PlantillasCorreoSeeder> log)
{
    public async Task EjecutarAsync(CancellationToken ct)
    {
        if (!await db.Database.CanConnectAsync(ct))
            return;

        try
        {
            await SincronizarAsync(ct);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "PlantillasCorreoSeeder omitido (¿falta migración?).");
        }
    }

    private async Task SincronizarAsync(CancellationToken ct)
    {
        var vigentes = CatalogoPlantillasCorreo.Todas.ToDictionary(d => d.Codigo);
        var enBd = await db.PlantillaCorreo.ToDictionaryAsync(p => p.Codigo, ct);

        foreach (var d in CatalogoPlantillasCorreo.Todas)
        {
            if (enBd.TryGetValue(d.Codigo, out var fila))
            {
                fila.Nombre = d.Nombre;
                fila.Descripcion = d.Descripcion;
                fila.Asunto = d.Asunto;
                fila.CuerpoHtml = d.CuerpoHtml;
                fila.Activo = true;
            }
            else
            {
                db.PlantillaCorreo.Add(new PlantillaCorreo
                {
                    Id = d.Id,
                    Codigo = d.Codigo,
                    Nombre = d.Nombre,
                    Descripcion = d.Descripcion,
                    Asunto = d.Asunto,
                    CuerpoHtml = d.CuerpoHtml,
                    Activo = true
                });
            }
        }

        foreach (var fila in enBd.Values.Where(p => !vigentes.ContainsKey(p.Codigo)))
            fila.Activo = false;

        await db.SaveChangesAsync(ct);
    }
}