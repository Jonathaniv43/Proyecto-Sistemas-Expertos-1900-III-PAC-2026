namespace Norvia.Domain.Entities.Acceso;

/// <summary>Cuenta del staff Norvia; login separado del de empresa (D21, D23).</summary>
public class StaffPlataforma
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public short RolStaffPlataformaId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool DebeCambiarPassword { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; }

    public RolStaffPlataforma RolStaffPlataforma { get; set; } = null!;
}
