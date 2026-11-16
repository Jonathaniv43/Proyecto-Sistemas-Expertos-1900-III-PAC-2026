using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ObtenerReporteEncuestaRepositorio(NorviaDbContext db) : IObtenerReporteEncuestaRepositorio
{
    public Task<Encuesta?> BuscarConVersionPublicadaAsync(Guid encuestaId, CancellationToken ct) =>
        db.Encuesta.AsNoTracking()
            .Include(e => e.Versiones.Where(v => v.Estado == EstadoVersionEncuesta.Publicada))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.Opciones)
            .Include(e => e.Versiones.Where(v => v.Estado == EstadoVersionEncuesta.Publicada))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.TipoRespuestaPregunta)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == encuestaId, ct);

    public Task<int> ContarParticipacionesAsync(Guid encuestaId, CancellationToken ct) =>
        db.ParticipacionEncuesta.CountAsync(p => p.EncuestaId == encuestaId, ct);

    public Task<List<RespuestaEncuesta>> ListarRespuestasDeVersionAsync(Guid versionId, CancellationToken ct) =>
        db.RespuestaEncuesta.AsNoTracking()
            .Where(r => r.EncuestaVersionId == versionId)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Opciones)
            .ToListAsync(ct);
}
