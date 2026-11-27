namespace Norvia.Domain.Enums;

/// <summary>Estado del envío por correo de una notificación (RN-78). La notificación en Norvia no depende de él.</summary>
public enum EstadoCorreo
{
    /// <summary>Falta enviarlo, o falló y se va a reintentar.</summary>
    Pendiente,
    Enviado,

    /// <summary>Se agotaron los reintentos. La notificación sigue visible en Norvia.</summary>
    Fallido,

    /// <summary>Un envío inmediato o un reintento manual la tiene tomada en este momento (guardo atómico, RN-78).</summary>
    EnProceso
}
