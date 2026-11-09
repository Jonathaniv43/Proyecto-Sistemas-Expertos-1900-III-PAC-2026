namespace Norvia.Domain.Entities.Riesgos;

/// <summary>Riesgo de la empresa: peligro ubicado en un área (D9, RN-17, RN-18).</summary>
public class RiesgoEmpresa : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid? PeligroId { get; set; }

    /// <summary>Para peligros que no están en el catálogo.</summary>
    public string? DescripcionPeligro { get; set; }
    public Guid AreaId { get; set; }
    public string? Actividad { get; set; }
    public EstadoRiesgo Estado { get; set; }

    /// <summary>Encuesta que lo sugirió. Sin navegación hasta implementar M3.</summary>
    public Guid? EncuestaId { get; set; }
    public DateTime CreadoEn { get; set; }
    public Guid? RevisadoPorId { get; set; }
    public DateTime? RevisadoEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Peligro? Peligro { get; set; }
    public Area Area { get; set; } = null!;
    public VinculoEmpresa? RevisadoPor { get; set; }
    public ICollection<EvaluacionRiesgo> Evaluaciones { get; set; } = new List<EvaluacionRiesgo>();
    public ICollection<Control> Controles { get; set; } = new List<Control>();
}
