namespace Norvia.Domain.Entities.Catalogos;

/// <summary>Rubro de actividad (catálogo global, D7).</summary>
public class Rubro
{
    public short Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<CategoriaRiesgo> Categorias { get; set; } = new List<CategoriaRiesgo>();
}
