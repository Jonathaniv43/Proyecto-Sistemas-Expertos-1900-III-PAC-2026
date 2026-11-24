using Norvia.Domain.Entities.Notificaciones;

namespace Norvia.Application.Features.Notificaciones.MarcarNotificacionLeida;

public interface IMarcarNotificacionLeidaRepositorio
{
    /// <summary>
    /// La notificación, con seguimiento de cambios, solo si es del destinatario indicado y de la empresa de la sesión.
    /// </summary>
    Task<Notificacion?> BuscarPropiaAsync(Guid id, Guid destinatarioId, CancellationToken ct);
}
