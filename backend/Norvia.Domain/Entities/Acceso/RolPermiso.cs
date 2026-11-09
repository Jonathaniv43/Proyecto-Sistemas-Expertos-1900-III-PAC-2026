namespace Norvia.Domain.Entities.Acceso;

/// <summary>Permisos de un rol de empresa; solo Alcance = Empresa (RN-63).</summary>
public class RolPermiso
{
    public short RolId { get; set; }
    public short PermisoId { get; set; }

    public Rol Rol { get; set; } = null!;
    public Permiso Permiso { get; set; } = null!;
}
