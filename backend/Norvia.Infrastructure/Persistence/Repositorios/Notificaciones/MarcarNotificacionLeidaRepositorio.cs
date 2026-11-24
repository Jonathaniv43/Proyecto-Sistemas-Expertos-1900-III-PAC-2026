using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Notificaciones.MarcarNotificacionLeida;
using Norvia.Domain.Entities.Notificaciones;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Notificaciones;

public sealed class MarcarNotificacionLeidaRepositorio(NorviaDbContext db) : IMarcarNotificacionLeidaRepositorio
{
    public Task<Notificacion?> BuscarPropiaAsync(Guid id, Guid destinatarioId, CancellationToken ct) =>
        db.Notificacion.FirstOrDefaultAsync(n => n.Id == id && n.DestinatarioId == destinatarioId, ct);
}
