namespace Norvia.Domain.Enums;

/// <summary>Estado de una versión de documento (D13, RN-49 a RN-51). Solo la versión en borrador se edita (RN-50).</summary>
public enum EstadoVersionDocumento
{
    /// <summary>Se puede editar; un documento tiene como máximo un borrador.</summary>
    Borrador,

    /// <summary>Aprobada y vigente; un documento tiene como máximo una (RN-51).</summary>
    Vigente,

    /// <summary>Fue vigente y la reemplazó una versión aprobada después. Se conserva para consulta.</summary>
    Reemplazada
}
