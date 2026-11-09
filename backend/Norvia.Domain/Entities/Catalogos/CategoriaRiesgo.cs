namespace Norvia.Domain.Entities.Catalogos;

/// <summary>Categoría de riesgo de un rubro (catálogo global, D7).</summary>
public class CategoriaRiesgo
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public short RubroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activa { get; set; } = true;

    public Rubro Rubro { get; set; } = null!;
    public ICollection<PreguntaExploratoria> Preguntas { get; set; } = new List<PreguntaExploratoria>();
}
