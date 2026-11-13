namespace Norvia.Domain.Entities.Acceso;

/// <summary>Sesión: en BD solo el hash del refresh. El JWT de access no se guarda.</summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid AccesoId { get; set; }
    public Guid? VinculoEmpresaId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public TipoSesion TipoSesion { get; set; }
    public DateTime ExpiraEn { get; set; }
    public DateTime CreadoEn { get; set; }

    public Acceso Acceso { get; set; } = null!;
    public VinculoEmpresa? VinculoEmpresa { get; set; }
}
