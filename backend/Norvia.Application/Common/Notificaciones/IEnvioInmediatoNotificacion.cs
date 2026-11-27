namespace Norvia.Application.Common.Notificaciones;

/// <summary>
/// Intento de envío del correo de una notificación fuera del ciclo del poller de fondo (RN-78): al crearla
/// (mejor esfuerzo, justo después de confirmar la operación de origen) o desde el botón "Reintentar" de
/// Norvia. Nunca lanza: si falla, la fila queda igual que si la hubiera tomado el poller. Un guardo atómico
/// evita que este camino y el poller procesen la misma fila a la vez.
/// </summary>
public interface IEnvioInmediatoNotificacion
{
    Task IntentarAsync(Guid notificacionId, CancellationToken ct);
}
