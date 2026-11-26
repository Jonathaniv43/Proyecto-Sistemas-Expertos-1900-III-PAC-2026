namespace Norvia.Domain.Entities.Catalogos;

/// <summary>Plantilla HTML de correo transaccional Norvia (catálogo global).</summary>
public class PlantillaCorreo
{
    public short Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string CuerpoHtml { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}