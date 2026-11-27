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
        // Por Id (la llave real), no por Codigo: un código puede reemplazar a otro reutilizando su fila
        // (p. ej. activacion_cuenta reemplazó a bienvenida_empresa con el mismo Id), y si se buscara por
        // Codigo, esa fila no se encontraría y el Add explícito de Id chocaría con la que ya está rastreada.
        var idsVigentes = CatalogoPlantillasCorreo.Todas.Select(d => d.Id).ToHashSet();
        var enBd = await db.PlantillaCorreo.ToDictionaryAsync(p => p.Id, ct);

        foreach (var d in CatalogoPlantillasCorreo.Todas)
        {
            if (enBd.TryGetValue(d.Id, out var fila))
            {
                fila.Codigo = d.Codigo;
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

        foreach (var fila in enBd.Values.Where(p => !idsVigentes.Contains(p.Id)))
            fila.Activo = false;

        await db.SaveChangesAsync(ct);
    }
}