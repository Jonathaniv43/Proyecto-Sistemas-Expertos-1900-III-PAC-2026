namespace Norvia.Application.Features.Notificaciones.ReintentarCorreo;

public interface IReintentarCorreoRepositorio
{
    /// <summary>true si la notificación existe y es del destinatario indicado, de la empresa de la sesión.</summary>
    Task<bool> EsPropiaAsync(Guid id, Guid destinatarioId, CancellationToken ct);

    /// <summary>
    /// Si el correo está pendiente o fallido, lo deja pendiente para ya (guardo atómico). No toca uno ya enviado
    /// ni uno que el proceso de fondo está enviando en este momento.
    /// </summary>
    Task ProgramarYaAsync(Guid id, DateTime ahora, CancellationToken ct);
}
