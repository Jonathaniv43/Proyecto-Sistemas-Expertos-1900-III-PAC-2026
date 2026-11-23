using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ListarEncuestasPendientes;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ListarEncuestasPendientesRepositorio(NorviaDbContext db) : IListarEncuestasPendientesRepositorio
{
    public Task<List<Encuesta>> ListarPublicadasSinParticipacionAsync(Guid vinculoEmpresaId, CancellationToken ct) =>
        db.Encuesta.AsNoTracking()
            .Where(e => e.Estado == EstadoEncuesta.Publicada)
            .Where(e => !db.ParticipacionEncuesta.Any(p =>
                p.EncuestaId == e.Id && p.VinculoEmpresaId == vinculoEmpresaId))
            .Include(e => e.Versiones.Where(v => v.Estado == EstadoVersionEncuesta.Publicada))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.Opciones)
            .Include(e => e.Versiones.Where(v => v.Estado == EstadoVersionEncuesta.Publicada))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.TipoRespuestaPregunta)
            .AsSplitQuery()
            .OrderByDescending(e => e.CreadaEn)
            .ToListAsync(ct);
}
