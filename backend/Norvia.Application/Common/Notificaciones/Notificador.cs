using System.Globalization;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Entities.Notificaciones;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;

namespace Norvia.Application.Common.Notificaciones;

/// <summary>
/// Crea las notificaciones de M11 (D28) con su texto. No guarda nada: la notificación se confirma junto con la
/// operación de origen (si esta falla, no queda un aviso suelto). Quien llama intenta el correo de inmediato
/// después de guardar (mejor esfuerzo, con <c>IEnvioInmediatoNotificacion</c>); si falla, el poller de fondo lo
/// reintenta después (RN-78). Si el correo falla, la operación de origen ya quedó hecha y no se revierte (HU-118).
/// </summary>
public sealed class Notificador(IBandejaNotificaciones bandeja, ITenantActual sesion)
{
    private static readonly CultureInfo Honduras = new("es-HN");

    /// <summary>
    /// RN-77, HU-081: avisa al responsable que se le asignó la tarea. No avisa a quien se la asigna a sí mismo.
    /// Null si no correspondía avisar.
    /// </summary>
    public Notificacion? TareaAsignada(Tarea tarea)
    {
        if (tarea.ResponsableId is not Guid responsable || responsable == sesion.VinculoEmpresaId) return null;

        var fecha = tarea.FechaLimite is { } f ? $" Fecha límite: {f.ToString("d 'de' MMMM 'de' yyyy", Honduras)}." : string.Empty;
        var notificacion = new Notificacion
        {
            TareaId = tarea.Id,
            DestinatarioId = responsable,
            Tipo = TipoNotificacion.TareaAsignada,
            Titulo = "Te asignaron una tarea",
            Mensaje = $"{tarea.Titulo}.{fecha}"
        };
        bandeja.Agregar(notificacion);
        return notificacion;
    }

    /// <summary>
    /// RN-75, HU-116: avisa a cada vínculo activo de la empresa que se aprobó una versión de una política.
    /// Quien llama (aprobar un documento, HU-072) pasa los destinatarios sin repetir; la base además lo impide.
    /// </summary>
    public IReadOnlyList<Notificacion> PoliticaPublicada(Guid versionDocumentoId, string tituloPolitica, IEnumerable<Guid> destinatarios)
    {
        var creadas = new List<Notificacion>();
        foreach (var destinatario in destinatarios.Distinct())
        {
            var notificacion = new Notificacion
            {
                VersionDocumentoId = versionDocumentoId,
                DestinatarioId = destinatario,
                Tipo = TipoNotificacion.PoliticaPublicada,
                Titulo = $"Nueva política: {tituloPolitica}",
                Mensaje = $"Se aprobó una nueva versión de «{tituloPolitica}». Puedes consultarla en Norvia."
            };
            bandeja.Agregar(notificacion);
            creadas.Add(notificacion);
        }
        return creadas;
    }
}
