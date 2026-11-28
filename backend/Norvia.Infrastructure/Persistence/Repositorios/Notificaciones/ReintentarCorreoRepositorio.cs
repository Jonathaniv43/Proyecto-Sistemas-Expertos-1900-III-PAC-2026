using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Notificaciones.ReintentarCorreo;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Notificaciones;

public sealed class ReintentarCorreoRepositorio(NorviaDbContext db) : IReintentarCorreoRepositorio
{
    public Task<bool> EsPropiaAsync(Guid id, Guid destinatarioId, CancellationToken ct) =>
        db.Notificacion.AnyAsync(n => n.Id == id && n.DestinatarioId == destinatarioId, ct);

    public Task ProgramarYaAsync(Guid id, DateTime ahora, CancellationToken ct) =>
        db.Notificacion
            .Where(n => n.Id == id && (n.CorreoEstado == EstadoCorreo.Pendiente || n.CorreoEstado == EstadoCorreo.Fallido))
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.CorreoEstado, EstadoCorreo.Pendiente)
                .SetProperty(n => n.CorreoProximoIntentoEn, ahora), ct);
}
