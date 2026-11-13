namespace Norvia.Domain.Entities.Acceso;

/// <summary>Correo + contraseña. Un email, una llave; varios vínculos pueden compartirla.</summary>
public class Acceso
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool DebeCambiarPassword { get; set; }
    public bool Verificado { get; set; }
    public DateTime CreadoEn { get; set; }

    public ICollection<VinculoEmpresa> Vinculos { get; set; } = new List<VinculoEmpresa>();
    public StaffPlataforma? Staff { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
