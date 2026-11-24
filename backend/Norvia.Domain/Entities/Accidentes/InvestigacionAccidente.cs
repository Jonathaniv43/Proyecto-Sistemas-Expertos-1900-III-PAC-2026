namespace Norvia.Domain.Entities.Accidentes;

/// <summary>
/// Investigación de un evento (HU-049, art. 46.1.a): causas inmediatas y de fondo, conclusiones, quién investigó y
/// cuándo. Una por evento. Las medidas correctivas son tareas con <c>AccidenteId</c> (HU-050).
/// </summary>
public class InvestigacionAccidente : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid AccidenteId { get; set; }
    public string CausasInmediatas { get; set; } = string.Empty;

    /// <summary>Causas de fondo.</summary>
    public string CausasBasicas { get; set; } = string.Empty;
    public string? Conclusiones { get; set; }
    public Guid InvestigadaPorId { get; set; }
    public DateOnly FechaInvestigacion { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Accidente Accidente { get; set; } = null!;
    public VinculoEmpresa InvestigadaPor { get; set; } = null!;
}
