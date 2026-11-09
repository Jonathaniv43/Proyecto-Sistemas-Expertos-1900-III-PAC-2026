namespace Norvia.Domain.Entities.Acceso;

/// <summary>Cambio de estado de un vínculo, con motivo obligatorio (D22, RN-59).</summary>
public class HistorialEstadoVinculo : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid VinculoEmpresaId { get; set; }
    public EstadoVinculo? EstadoAnterior { get; set; }
    public EstadoVinculo EstadoNuevo { get; set; }
    public MotivoEstadoVinculo MotivoCodigo { get; set; }
    public string? MotivoDetalle { get; set; }
    public Guid RegistradoPorId { get; set; }
    public DateTime RegistradoEn { get; set; }

    public VinculoEmpresa VinculoEmpresa { get; set; } = null!;
    public VinculoEmpresa RegistradoPor { get; set; } = null!;
}
