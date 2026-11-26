using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.CompararEncuestas;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class CompararEncuestasRepositorio(NorviaDbContext db) : ICompararEncuestasRepositorio
{
    public Task<EncuestaVersion?> BuscarVersionAsync(Guid versionId, CancellationToken ct) =>
        db.EncuestaVersion.AsNoTracking()
            .Include(v => v.Encuesta)
            .Include(v => v.Categorias)
                .ThenInclude(c => c.CategoriaRiesgo)
            .Include(v => v.Preguntas)
                .ThenInclude(p => p.Opciones)
            .Include(v => v.Preguntas)
                .ThenInclude(p => p.TipoRespuestaPregunta)
            .Include(v => v.Preguntas)
                .ThenInclude(p => p.Peligro)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == versionId, ct);

    public Task<int> ContarParticipacionesAsync(Guid encuestaId, CancellationToken ct) =>
        db.ParticipacionEncuesta.CountAsync(p => p.EncuestaId == encuestaId, ct);

    public Task<List<RespuestaEncuesta>> ListarRespuestasDeVersionAsync(Guid versionId, CancellationToken ct) =>
        db.RespuestaEncuesta.AsNoTracking()
            .Where(r => r.EncuestaVersionId == versionId)
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Opciones)
            .ToListAsync(ct);

    public Task<Dictionary<Guid, EstadoRiesgo>> EstadosRiesgosPorPeligroAsync(Guid encuestaId, CancellationToken ct) =>
        db.RiesgoEmpresa.AsNoTracking()
            .Where(r => r.EncuestaId == encuestaId && r.PeligroId != null)
            .ToDictionaryAsync(r => r.PeligroId!.Value, r => r.Estado, ct);
}
