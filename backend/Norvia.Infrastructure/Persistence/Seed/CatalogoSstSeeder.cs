using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>
/// Carga el contenido SST inicial (<see cref="CatalogoSst"/>, HU-097) en los catálogos globales.
/// <list type="bullet">
/// <item><b>Solo agrega lo que falta:</b> cada elemento tiene un Id estable derivado de su clave, así una carga repetida
/// no duplica nada y no pisa lo que el staff haya editado o desactivado (HU-023).</item>
/// <item><b>Respeta lo cargado a mano:</b> si ya existe un peligro con el mismo código o una categoría con el mismo nombre
/// en el rubro, se usa ese en lugar de crear otro; si la posición de una pregunta ya está ocupada, esa pregunta no se agrega.</item>
/// <item>Necesita los rubros (migración) y los tipos de respuesta (<see cref="TiposRespuestaPreguntaSeeder"/>, que corre antes).</item>
/// </list>
/// </summary>
public sealed class CatalogoSstSeeder(NorviaDbContext db, IOptions<CatalogoSstOpciones> opciones, ILogger<CatalogoSstSeeder> log)
{
    public async Task EjecutarAsync(CancellationToken ct)
    {
        if (!opciones.Value.Cargar || !await db.Database.CanConnectAsync(ct))
            return;

        try
        {
            var agregados = await SembrarAsync(ct);
            if (agregados > 0) log.LogInformation("Catálogo SST inicial: {Agregados} elementos agregados.", agregados);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "CatalogoSstSeeder omitido (¿falta migración?).");
        }
    }

    /// <summary>Id estable (UUID v5 sobre SHA-1) a partir de la clave del elemento.</summary>
    public static Guid IdDe(string clave)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes($"norvia:catalogo-sst:{clave}"));
        var bytes = hash[..16];
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes, bigEndian: true);
    }

    /// <summary>
    /// Clave del Id de un peligro y sus plantillas: el código en la forma de la primera versión (<c>LOC-CAIDA-NIVEL</c>).
    /// Así una base ya sembrada conserva sus Id aunque el código haya pasado a minúsculas con guion bajo.
    /// </summary>
    private static string ClavePeligro(string codigo) => codigo.ToUpperInvariant().Replace('_', '-');

    private async Task<int> SembrarAsync(CancellationToken ct)
    {
        var agregados = 0;
        var rubros = await db.Rubro.AsNoTracking().ToDictionaryAsync(r => r.Codigo, r => r.Id, ct);
        var tipos = await db.TipoRespuestaPregunta.AsNoTracking().ToDictionaryAsync(t => t.Codigo, t => t.Id, ct);

        // Peligros y sus plantillas.
        var peligrosPorId = await db.Peligro.AsNoTracking().Select(p => p.Id).ToHashSetAsync(ct);
        var peligrosPorCodigo = await db.Peligro.AsNoTracking().ToDictionaryAsync(p => p.Codigo, p => p.Id, ct);
        var plantillas = await db.PlantillaTarea.AsNoTracking().Select(p => p.Id).ToHashSetAsync(ct);
        var peligroId = new Dictionary<string, Guid>();
        foreach (var p in CatalogoSst.Peligros)
        {
            var id = IdDe($"peligro:{ClavePeligro(p.Codigo)}");
            if (!peligrosPorId.Contains(id) && peligrosPorCodigo.TryGetValue(p.Codigo, out var existente))
                id = existente;
            else if (!peligrosPorId.Contains(id))
            {
                db.Peligro.Add(new Peligro { Id = id, Codigo = p.Codigo, Nombre = p.Nombre, Descripcion = p.Descripcion, Clasificacion = p.Clasificacion });
                agregados++;
            }
            peligroId[p.Codigo] = id;

            for (var i = 0; i < p.Plantillas.Length; i++)
            {
                var idPlantilla = IdDe($"plantilla:{ClavePeligro(p.Codigo)}:{i + 1}");
                if (plantillas.Contains(idPlantilla)) continue;
                var t = p.Plantillas[i];
                db.PlantillaTarea.Add(new PlantillaTarea
                {
                    Id = idPlantilla, PeligroId = id, TipoControl = t.Tipo, Titulo = t.Titulo, Descripcion = t.Descripcion,
                    PrioridadSugerida = t.Prioridad, RequiereEvidencia = t.Evidencia
                });
                agregados++;
            }
        }

        // Categorías por rubro, con sus preguntas y opciones.
        var categorias = await db.CategoriaRiesgo.AsNoTracking().Select(c => new { c.Id, c.RubroId, c.Nombre }).ToListAsync(ct);
        var preguntas = await db.PreguntaExploratoria.AsNoTracking().Select(p => new { p.Id, p.CategoriaRiesgoId, p.Orden }).ToListAsync(ct);
        var idsPregunta = preguntas.Select(p => p.Id).ToHashSet();
        var posiciones = preguntas.Select(p => (p.CategoriaRiesgoId, p.Orden)).ToHashSet();
        foreach (var c in CatalogoSst.Categorias)
        {
            var rubroId = rubros[c.RubroCodigo];
            var idCategoria = IdDe($"categoria:{c.RubroCodigo}:{c.Clave}");
            if (categorias.FirstOrDefault(x => x.Id == idCategoria) is null)
            {
                if (categorias.FirstOrDefault(x => x.RubroId == rubroId && x.Nombre == c.Nombre) is { } mismaNombre)
                    idCategoria = mismaNombre.Id;
                else
                {
                    db.CategoriaRiesgo.Add(new CategoriaRiesgo { Id = idCategoria, RubroId = rubroId, Nombre = c.Nombre, Descripcion = c.Descripcion });
                    agregados++;
                }
            }

            for (short orden = 1; orden <= c.Preguntas.Length; orden++)
            {
                var idPregunta = IdDe($"pregunta:{c.RubroCodigo}:{c.Clave}:{orden}");
                if (idsPregunta.Contains(idPregunta) || posiciones.Contains((idCategoria, orden))) continue;
                var q = c.Preguntas[orden - 1];
                var pregunta = new PreguntaExploratoria
                {
                    Id = idPregunta, CategoriaRiesgoId = idCategoria, Texto = q.Texto, TipoRespuestaPreguntaId = tipos[q.TipoCodigo],
                    Obligatoria = q.Obligatoria, Orden = orden, PeligroId = q.PeligroCodigo is null ? null : peligroId[q.PeligroCodigo]
                };
                for (short o = 1; o <= q.Opciones.Length; o++)
                    pregunta.Opciones.Add(new OpcionPregunta
                    {
                        Id = IdDe($"opcion:{c.RubroCodigo}:{c.Clave}:{orden}:{o}"), Texto = q.Opciones[o - 1].Texto, Orden = o,
                        IndicaExposicion = q.Opciones[o - 1].IndicaExposicion
                    });
                db.PreguntaExploratoria.Add(pregunta);
                agregados++;
            }
        }

        if (agregados > 0) await db.SaveChangesAsync(ct);
        return agregados;
    }
}
