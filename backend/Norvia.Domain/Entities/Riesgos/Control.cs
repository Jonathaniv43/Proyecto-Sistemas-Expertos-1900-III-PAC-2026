using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Domain.Entities.Riesgos;

/// <summary>
/// Medida de control de un riesgo (RN-21). <see cref="PlantillaTareaId"/> indica la plantilla del peligro de la que
/// salió cuando se aceptó una sugerencia (RN-76); null si el control es propio.
/// </summary>
public class Control : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid RiesgoEmpresaId { get; set; }
    public Guid? PlantillaTareaId { get; set; }
    public TipoControl Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public EstadoControl Estado { get; set; } = EstadoControl.Propuesto;
    public DateTime CreadoEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public RiesgoEmpresa RiesgoEmpresa { get; set; } = null!;
    public PlantillaTarea? PlantillaTarea { get; set; }
}
