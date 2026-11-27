using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Notificaciones.ReintentarCorreo;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Notificaciones;

public sealed class ReintentarCorreoRepositorio(NorviaDbContext db) : IReintentarCorreoRepositorio
{
    public Task<bool> EsPropiaAsync(Guid id, Guid destinatarioId, CancellationToken ct) =>
        db.Notificacion.AnyAsync(n => n.Id == id && n.DestinatarioId == destinatarioId, ct);
}
