using Norvia.Domain.Entities.Notificaciones;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Notificaciones;

/// <summary>Consultas que comparten los repositorios del módulo Notificaciones.</summary>
internal static class ConsultasNotificacion
{
    /// <summary>No leídas del destinatario; el filtro global ya limita a la empresa de la sesión.</summary>
    public static IQueryable<Notificacion> NoLeidas(NorviaDbContext db, Guid destinatarioId) =>
        db.Notificacion.Where(n => n.DestinatarioId == destinatarioId && n.LeidaEn == null);
}
