namespace Norvia.Domain.Entities.Acceso;

/// <summary>Membresía en una empresa. Credenciales en Acceso (D1, D21).</summary>
public class VinculoEmpresa : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid PersonaId { get; set; }
    public Guid AccesoId { get; set; }
    public short RolId { get; set; }
    public EstadoVinculo Estado { get; set; } = EstadoVinculo.Activo;
    public DateTime CreadoEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Persona Persona { get; set; } = null!;
    public Acceso Acceso { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
    public ICollection<HistorialEstadoVinculo> HistorialEstados { get; set; } = new List<HistorialEstadoVinculo>();
}
