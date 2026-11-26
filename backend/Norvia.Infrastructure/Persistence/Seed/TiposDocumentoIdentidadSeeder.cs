using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>Sincroniza la tabla <see cref="TipoDocumentoIdentidadCatalogo"/> con <see cref="CatalogoTiposDocumentoIdentidad"/>.</summary>
public sealed class TiposDocumentoIdentidadSeeder(NorviaDbContext db, ILogger<TiposDocumentoIdentidadSeeder> log)
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
            log.LogWarning(ex, "TiposDocumentoIdentidadSeeder omitido (¿falta migración?).");
        }
    }

    private async Task SincronizarAsync(CancellationToken ct)
    {
        var enBd = await db.TipoDocumentoIdentidad.ToDictionaryAsync(t => t.Codigo, ct);

        foreach (var (codigo, nombre) in CatalogoTiposDocumentoIdentidad.Filas)
        {
            if (enBd.TryGetValue(codigo, out var fila))
                fila.Nombre = nombre;
            else
                db.TipoDocumentoIdentidad.Add(new TipoDocumentoIdentidadCatalogo
                {
                    Id = Guid.CreateVersion7(),
                    Codigo = codigo,
                    Nombre = nombre,
                });
        }

        await db.SaveChangesAsync(ct);
    }
}