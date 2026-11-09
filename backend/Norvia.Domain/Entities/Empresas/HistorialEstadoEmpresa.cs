namespace Norvia.Domain.Entities.Empresas;

/// <summary>Suspensión o reactivación del tenant (D22).</summary>
public class HistorialEstadoEmpresa : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public EstadoEmpresa? EstadoAnterior { get; set; }
    public EstadoEmpresa EstadoNuevo { get; set; }
    public MotivoEstadoEmpresa MotivoCodigo { get; set; }
    public string? MotivoDetalle { get; set; }

    /// <summary>Staff Norvia que registró el cambio; nulo si fue un proceso automático.</summary>
    public Guid? RegistradoPorId { get; set; }
    public DateTime RegistradoEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public StaffPlataforma? RegistradoPor { get; set; }
}
