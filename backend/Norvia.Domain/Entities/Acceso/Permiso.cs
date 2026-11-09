namespace Norvia.Domain.Entities.Acceso;

/// <summary>Operación de la API y tipo de sesión en que aplica (D20, D24).</summary>
public class Permiso
{
    public short Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public AlcancePermiso Alcance { get; set; }
    public string MetodoHttp { get; set; } = string.Empty;
    public string RutaPatron { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<RolPermiso> Roles { get; set; } = new List<RolPermiso>();
    public ICollection<RolStaffPlataformaPermiso> RolesStaff { get; set; } = new List<RolStaffPlataformaPermiso>();
}
