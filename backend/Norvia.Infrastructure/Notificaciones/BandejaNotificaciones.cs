using Norvia.Application.Common.Notificaciones;
using Norvia.Domain.Entities.Notificaciones;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Notificaciones;

/// <summary>Agrega la notificación al contexto; EmpresaId lo asigna el interceptor al guardar (RN-06).</summary>
public sealed class BandejaNotificaciones(NorviaDbContext db) : IBandejaNotificaciones
{
    public void Agregar(Notificacion notificacion) => db.Notificacion.Add(notificacion);
}
