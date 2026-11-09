namespace Norvia.Domain.Entities.Acceso;

/// <summary>Rol global del staff Norvia (D23).</summary>
public class RolStaffPlataforma
{
    public short Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public ICollection<StaffPlataforma> Staff { get; set; } = new List<StaffPlataforma>();
    public ICollection<RolStaffPlataformaPermiso> Permisos { get; set; } = new List<RolStaffPlataformaPermiso>();
}
