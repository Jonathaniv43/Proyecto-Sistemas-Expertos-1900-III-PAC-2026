namespace Norvia.Domain.Entities.Acceso;

/// <summary>Rol dentro de una empresa (VinculoEmpresa.RolId).</summary>
public class Rol
{
    public short Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public ICollection<VinculoEmpresa> Vinculos { get; set; } = new List<VinculoEmpresa>();
    public ICollection<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();
}
