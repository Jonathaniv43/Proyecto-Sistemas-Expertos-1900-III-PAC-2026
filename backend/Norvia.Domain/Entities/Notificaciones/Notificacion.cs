using Norvia.Domain.Common;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Empresas;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;

namespace Norvia.Domain.Entities.Notificaciones;

/// <summary>
/// Aviso a un operador con vínculo (D28, 12-modelo-datos § Notificacion). Se ve dentro de Norvia y se envía por correo;
/// la misma fila es la bandeja de salida del correo (RN-78): si el envío falla, se reintenta sin perder el aviso.
/// <para>
/// <see cref="Titulo"/> y <see cref="Mensaje"/> guardan el texto tal como se envió, aunque la tarea o el documento
/// cambien después.
/// </para>
/// </summary>
public class Notificacion : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid DestinatarioId { get; set; }
    public TipoNotificacion Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Origen: versión de política aprobada (RN-75). Todavía sin FK: la tabla VersionDocumento llega con HU-070,
    /// y en esa migración se agrega la FK compuesta (EmpresaId, VersionDocumentoId).
    /// </summary>
    public Guid? VersionDocumentoId { get; set; }

    /// <summary>Origen: tarea asignada (RN-77).</summary>
    public Guid? TareaId { get; set; }

    public DateTime CreadaEn { get; set; }
    public DateTime? LeidaEn { get; set; }

    public EstadoCorreo CorreoEstado { get; set; } = EstadoCorreo.Pendiente;
    public short CorreoIntentos { get; set; }

    /// <summary>
    /// Desde cuándo se puede volver a intentar el correo: la espera crece con cada intento fallido. Nulo = cuanto antes.
    /// </summary>
    public DateTime? CorreoProximoIntentoEn { get; set; }
    public DateTime? CorreoEnviadoEn { get; set; }
    public string? CorreoUltimoError { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public VinculoEmpresa Destinatario { get; set; } = null!;
    public Tarea? Tarea { get; set; }
}
