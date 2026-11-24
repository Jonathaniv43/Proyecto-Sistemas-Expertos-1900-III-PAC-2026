namespace Norvia.Domain.Enums;

/// <summary>Origen de una notificación (D28): de él depende qué columna de origen tiene valor.</summary>
public enum TipoNotificacion
{
    /// <summary>Se aprobó una versión de una política (RN-75, HU-116).</summary>
    PoliticaPublicada,

    /// <summary>Se asignó una tarea a su responsable (RN-77, HU-081).</summary>
    TareaAsignada
}
