namespace Norvia.Domain.Entities.Acceso;

/// <summary>Correo + contraseña (opcional si entra con un proveedor, D29). Un email, una llave; varios vínculos pueden compartirla.</summary>
public class Acceso
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Email { get; set; } = string.Empty;
    /// <summary>Nulo en cuentas creadas con un proveedor de identidad (RN-81: debe haber contraseña o proveedor).</summary>
    public string? PasswordHash { get; set; }
    public bool DebeCambiarPassword { get; set; }
    public bool Verificado { get; set; }
    public DateTime CreadoEn { get; set; }

    public ICollection<VinculoEmpresa> Vinculos { get; set; } = new List<VinculoEmpresa>();
    public ICollection<AccesoProveedor> Proveedores { get; set; } = new List<AccesoProveedor>();
    public StaffPlataforma? Staff { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
