using Norvia.Application.Features.Notificaciones.DTOs;

namespace Norvia.Application.Features.Notificaciones.ListarMisNotificaciones;

public interface IListarMisNotificacionesRepositorio
{
    /// <summary>Las del destinatario en la empresa de la sesión, de la más reciente a la más antigua.</summary>
    Task<IReadOnlyList<NotificacionResponseDto>> ListarAsync(Guid destinatarioId, int limite, CancellationToken ct);

    Task<int> ContarNoLeidasAsync(Guid destinatarioId, CancellationToken ct);
}
