using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Encuestas.ResponderEncuesta;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Encuestas;

public sealed class ResponderEncuestaRepositorio(NorviaDbContext db) : IResponderEncuestaRepositorio
{
    public Task<Encuesta?> BuscarConVersionPublicadaAsync(Guid encuestaId, CancellationToken ct) =>
        db.Encuesta
            .Include(e => e.Versiones.Where(v => v.Estado == EstadoVersionEncuesta.Publicada))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.Opciones)
            .Include(e => e.Versiones.Where(v => v.Estado == EstadoVersionEncuesta.Publicada))
                .ThenInclude(v => v.Preguntas)
                    .ThenInclude(p => p.TipoRespuestaPregunta)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == encuestaId, ct);

    public Task<bool> YaParticipoAsync(Guid encuestaId, Guid vinculoEmpresaId, CancellationToken ct) =>
        db.ParticipacionEncuesta.AnyAsync(
            p => p.EncuestaId == encuestaId && p.VinculoEmpresaId == vinculoEmpresaId, ct);

    public void Agregar(RespuestaEncuesta respuesta) => db.RespuestaEncuesta.Add(respuesta);

    public void Agregar(ParticipacionEncuesta participacion) =>
        db.ParticipacionEncuesta.Add(participacion);
}
