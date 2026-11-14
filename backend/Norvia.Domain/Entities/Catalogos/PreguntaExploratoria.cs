namespace Norvia.Domain.Entities.Catalogos;

/// <summary>Pregunta de una categoría; puede indicar el peligro que detecta (D7).</summary>
public class PreguntaExploratoria
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid CategoriaRiesgoId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public TipoRespuestaPregunta TipoRespuesta { get; set; }
    public bool Obligatoria { get; set; } = true;
    public short Orden { get; set; }
    public Guid? PeligroId { get; set; }
    public bool Activa { get; set; } = true;

    public CategoriaRiesgo CategoriaRiesgo { get; set; } = null!;
    public Peligro? Peligro { get; set; }
    public ICollection<OpcionPregunta> Opciones { get; set; } = new List<OpcionPregunta>();
}
