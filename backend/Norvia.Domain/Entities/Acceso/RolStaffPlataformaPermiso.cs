namespace Norvia.Domain.Entities.Acceso;

/// <summary>Permisos de un rol del staff; solo Alcance = Plataforma (RN-63, RN-64).</summary>
public class RolStaffPlataformaPermiso
{
    public short RolStaffPlataformaId { get; set; }
    public short PermisoId { get; set; }

    public RolStaffPlataforma RolStaffPlataforma { get; set; } = null!;
    public Permiso Permiso { get; set; } = null!;
}
