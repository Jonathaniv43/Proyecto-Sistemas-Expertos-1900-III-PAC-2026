namespace Norvia.Application.Features.Notificaciones.ReintentarCorreo;

public interface IReintentarCorreoRepositorio
{
    /// <summary>true si la notificación existe y es del destinatario indicado, de la empresa de la sesión.</summary>
    Task<bool> EsPropiaAsync(Guid id, Guid destinatarioId, CancellationToken ct);
}
