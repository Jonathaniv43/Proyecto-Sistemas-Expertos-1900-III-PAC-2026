namespace Norvia.Domain.Entities.Catalogos;

/// <summary>Peligro del catálogo global (D9).</summary>
public class Peligro
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public ClasificacionPeligro Clasificacion { get; set; }
    public bool Activo { get; set; } = true;
}
