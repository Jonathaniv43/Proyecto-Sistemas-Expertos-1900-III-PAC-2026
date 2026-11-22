namespace Norvia.Domain.Enums;

/// <summary>Ciclo de vida de un evento reportado en M6 (RN-28).</summary>
public enum EstadoAccidente
{
    Abierto,

    // Pendiente HU-049: transiciones de investigación, cierre y descarte de eventos.
    EnInvestigacion,
    Cerrado,
    Descartado
}
