
namespace Norvia.Domain.Entities.Plan;

/// <summary>
/// Tarea del plan de trabajo, de cualquier módulo (D6). El origen se deduce de qué columna tiene valor:
/// <see cref="RiesgoEmpresaId"/> (riesgo, y <see cref="ControlId"/> si implementa un control), <see cref="AccidenteId"/>
/// (medida correctiva) o ninguna (manual) (RN-23).
/// </summary>
public class Tarea : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;

    /// <summary>De 1 (alta) a 3 (baja).</summary>
    public short Prioridad { get; set; }
    public Guid? ResponsableId { get; set; }
    public DateOnly? FechaLimite { get; set; }
    public bool RequiereEvidencia { get; set; }
    public Guid? PlantillaTareaId { get; set; }
    public Guid? RiesgoEmpresaId { get; set; }
    public Guid? ControlId { get; set; }
    public Guid? AccidenteId { get; set; }
    public Guid CreadaPorId { get; set; }
    public DateTime CreadaEn { get; set; }
    public DateTime? CompletadaEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public VinculoEmpresa? Responsable { get; set; }
    public PlantillaTarea? PlantillaTarea { get; set; }
    public RiesgoEmpresa? RiesgoEmpresa { get; set; }
    public Control? Control { get; set; }
    public Accidente? Accidente { get; set; }
    public VinculoEmpresa CreadaPor { get; set; } = null!;
}
