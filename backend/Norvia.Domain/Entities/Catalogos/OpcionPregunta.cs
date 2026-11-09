namespace Norvia.Domain.Entities.Catalogos;

/// <summary>Opción de respuesta de una pregunta exploratoria.</summary>
public class OpcionPregunta
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid PreguntaExploratoriaId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public short Orden { get; set; }

    /// <summary>True si elegir esta opción sugiere el peligro de la pregunta.</summary>
    public bool IndicaExposicion { get; set; }

    public PreguntaExploratoria PreguntaExploratoria { get; set; } = null!;
}
