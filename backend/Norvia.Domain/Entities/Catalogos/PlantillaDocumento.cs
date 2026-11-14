namespace Norvia.Domain.Entities.Catalogos;

/// <summary>
/// Política o procedimiento modelo (catálogo global, D13). El contenido lleva marcadores
/// como <c>{{RazonSocial}}</c> que se reemplazan con datos de la empresa.
/// </summary>
public class PlantillaDocumento
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public TipoDocumento TipoDocumento { get; set; }

    /// <summary>Nulo si aplica a todos los rubros.</summary>
    public short? RubroId { get; set; }

    public string Contenido { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;

    public Rubro? Rubro { get; set; }
}
