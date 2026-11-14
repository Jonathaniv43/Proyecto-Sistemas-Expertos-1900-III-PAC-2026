namespace Norvia.Domain.Entities.Acceso;

/// <summary>Staff Norvia; credenciales en Acceso (D21, D23).</summary>
public class StaffPlataforma
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid AccesoId { get; set; }
    public short RolStaffPlataformaId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; }

    public Acceso Acceso { get; set; } = null!;
    public RolStaffPlataforma RolStaffPlataforma { get; set; } = null!;
}
