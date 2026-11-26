using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>Sincroniza la tabla <see cref="TipoRespuestaPreguntaCatalogo"/> con <see cref="CatalogoTiposRespuestaPregunta"/>.</summary>
public sealed class TiposRespuestaPreguntaSeeder(NorviaDbContext db, ILogger<TiposRespuestaPreguntaSeeder> log)
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
            log.LogWarning(ex, "TiposRespuestaPreguntaSeeder omitido (¿falta migración?).");
        }
    }

    private async Task SincronizarAsync(CancellationToken ct)
    {
        var enBd = await db.TipoRespuestaPregunta.ToDictionaryAsync(t => t.Codigo, ct);

        foreach (var (codigo, nombre, instruccion, requiereOpciones, maxCaracteres) in CatalogoTiposRespuestaPregunta.Filas)
        {
            if (enBd.TryGetValue(codigo, out var fila))
            {
                fila.Nombre = nombre;
                fila.InstruccionUi = instruccion;
                fila.RequiereOpciones = requiereOpciones;
                fila.MaxCaracteresRespuesta = maxCaracteres;
                fila.Activo = true;
            }
            else
            {
                db.TipoRespuestaPregunta.Add(new TipoRespuestaPreguntaCatalogo
                {
                    Id = Guid.CreateVersion7(),
                    Codigo = codigo,
                    Nombre = nombre,
                    InstruccionUi = instruccion,
                    RequiereOpciones = requiereOpciones,
                    MaxCaracteresRespuesta = maxCaracteres,
                    Activo = true,
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
