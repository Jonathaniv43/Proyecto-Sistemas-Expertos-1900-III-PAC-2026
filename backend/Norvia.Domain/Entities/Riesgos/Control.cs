namespace Norvia.Domain.Entities.Riesgos;

/// <summary>Medida de control de un riesgo (RN-21).</summary>
public class Control : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid RiesgoEmpresaId { get; set; }
    public TipoControl Tipo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public EstadoControl Estado { get; set; } = EstadoControl.Propuesto;
    public DateTime CreadoEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public RiesgoEmpresa RiesgoEmpresa { get; set; } = null!;
}
