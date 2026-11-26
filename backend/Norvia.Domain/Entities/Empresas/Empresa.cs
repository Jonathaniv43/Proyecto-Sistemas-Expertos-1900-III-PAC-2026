namespace Norvia.Domain.Entities.Empresas;

/// <summary>Tabla raíz del tenant. No implementa ITieneEmpresa: es la empresa.</summary>
public class Empresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string RazonSocial { get; set; } = string.Empty;
    public string RTN { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? ActividadEconomica { get; set; }
    public short? RubroId { get; set; }
    public int? TrabajadoresPermanentes { get; set; }
    public DateOnly? FechaInicioOperaciones { get; set; }
    public EstadoEmpresa Estado { get; set; } = EstadoEmpresa.Activa;
    public DateTime CreadaEn { get; set; }

    public Rubro? Rubro { get; set; }
    public ICollection<VinculoEmpresa> Vinculos { get; set; } = new List<VinculoEmpresa>();
    public ICollection<HistorialEstadoEmpresa> HistorialEstados { get; set; } = new List<HistorialEstadoEmpresa>();
}
