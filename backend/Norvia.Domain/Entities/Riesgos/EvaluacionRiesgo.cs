namespace Norvia.Domain.Entities.Riesgos;

/// <summary>Evaluación de un riesgo; una nueva no reemplaza la anterior (D10, RN-20).</summary>
public class EvaluacionRiesgo : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid RiesgoEmpresaId { get; set; }
    public TipoEvaluacion Tipo { get; set; }

    /// <summary>De 1 a 5.</summary>
    public short Probabilidad { get; set; }

    /// <summary>De 1 a 5.</summary>
    public short Severidad { get; set; }
    public Guid EvaluadoPorId { get; set; }
    public DateTime EvaluadoEn { get; set; }

    /// <summary>Probabilidad × Severidad; se calcula y no se guarda (RN-15).</summary>
    public int Nivel => Probabilidad * Severidad;

    public Empresa Empresa { get; set; } = null!;
    public RiesgoEmpresa RiesgoEmpresa { get; set; } = null!;
    public VinculoEmpresa EvaluadoPor { get; set; } = null!;
}
