using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ActualizarBorradorEncuesta;
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
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.Opciones)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == encuestaId, ct);

    public async Task<bool> ExistenPeligrosActivosAsync(IReadOnlyCollection<Guid> peligroIds, CancellationToken ct) =>
        await db.Peligro.CountAsync(p => p.Activo && peligroIds.Contains(p.Id), ct) == peligroIds.Count;

    public void AgregarVersion(EncuestaVersion version) => db.EncuestaVersion.Add(version);

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
