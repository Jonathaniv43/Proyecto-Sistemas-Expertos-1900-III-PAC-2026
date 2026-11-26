namespace Norvia.Domain.Entities.Catalogos;

/// <summary>
/// Tarea modelo asociada a un peligro (catálogo global). Sirve para sugerir controles (RN-76)
/// y para generar el plan de trabajo (M5).
/// </summary>
public class PlantillaTarea
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid? PeligroId { get; set; }
    public TipoControl? TipoControl { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public short PrioridadSugerida { get; set; }
    public bool RequiereEvidencia { get; set; }
    public bool Activa { get; set; } = true;

    public Peligro? Peligro { get; set; }
}
