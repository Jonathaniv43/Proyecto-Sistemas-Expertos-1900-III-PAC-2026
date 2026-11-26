namespace Norvia.Domain.Entities.Acceso;

/// <summary>
/// Proveedor de identidad vinculado a un <see cref="Acceso"/> (D29). Global: no lleva EmpresaId.
/// El correo sigue solo en <see cref="Acceso.Email"/>.
/// </summary>
public class AccesoProveedor
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid AccesoId { get; set; }
    public ProveedorIdentidad Proveedor { get; set; }

    /// <summary>Identificador estable de la cuenta en el proveedor (claim <c>sub</c>).</summary>
    public string ProveedorUserId { get; set; } = string.Empty;
    public DateTime CreadoEn { get; set; }

    public Acceso Acceso { get; set; } = null!;
}
