namespace Norvia.Application.Features.Notificaciones.ContarNoLeidas;

public interface IContarNoLeidasRepositorio
{
    /// <summary>No leídas del destinatario en la empresa de la sesión.</summary>
    Task<int> ContarAsync(Guid destinatarioId, CancellationToken ct);
}
