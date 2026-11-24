using Norvia.Domain.Entities.Notificaciones;

namespace Norvia.Application.Common.Notificaciones;

/// <summary>
/// Donde quedan las notificaciones nuevas (D28). Solo las agrega al contexto: se guardan con la operación que las
/// originó, en el mismo <c>GuardarCambiosAsync</c>, y el correo lo envía después un proceso aparte (HU-118).
/// </summary>
public interface IBandejaNotificaciones
{
    void Agregar(Notificacion notificacion);
}
