using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ActualizarBorradorEncuestaRepositorio(NorviaDbContext db) : IActualizarBorradorEncuestaRepositorio
{
    public Task<Encuesta?> BuscarConVersionesAsync(Guid encuestaId, CancellationToken ct) =>
        db.Encuesta
            .Include(e => e.Versiones)
                .ThenInclude(v => v.Categorias)
            .Include(e => e.Versiones)
                .ThenInclude(v => v.Secciones)
            .Include(e => e.Versiones)
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.Opciones)
            .Include(e => e.Versiones)
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.TipoRespuestaPregunta)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == encuestaId, ct);

    public async Task<bool> ExistenPeligrosActivosAsync(IReadOnlyCollection<Guid> peligroIds, CancellationToken ct) =>
        await db.Peligro.CountAsync(p => p.Activo && peligroIds.Contains(p.Id), ct) == peligroIds.Count;

    public async Task<IReadOnlyDictionary<string, TipoRespuestaPreguntaCatalogo>> TiposPorCodigoAsync(CancellationToken ct)
    {
        var tipos = await db.TipoRespuestaPregunta.AsNoTracking().Where(t => t.Activo).ToListAsync(ct);
        return tipos.ToDictionary(t => t.Codigo, StringComparer.Ordinal);
    }

    public void AgregarVersion(EncuestaVersion version) => db.EncuestaVersion.Add(version);

    public void ReemplazarSecciones(EncuestaVersion borrador, IEnumerable<EncuestaSeccion> nuevasSecciones)
    {
        foreach (var vieja in borrador.Secciones.ToList())
            db.EncuestaSeccion.Remove(vieja);
        borrador.Secciones.Clear();

        foreach (var nueva in nuevasSecciones)
        {
            borrador.Secciones.Add(nueva);
            db.EncuestaSeccion.Add(nueva);
        }
    }

    public void ReemplazarPreguntas(EncuestaVersion borrador, IEnumerable<EncuestaPregunta> nuevasPreguntas)
    {
        foreach (var vieja in borrador.Preguntas.ToList())
        {
            db.EncuestaOpcion.RemoveRange(vieja.Opciones);
            db.EncuestaPregunta.Remove(vieja);
        }
        borrador.Preguntas.Clear();

        foreach (var nueva in nuevasPreguntas)
        {
            borrador.Preguntas.Add(nueva);
            db.EncuestaPregunta.Add(nueva);
        }
    }
}
