namespace Norvia.Domain.Entities.Acceso;

/// <summary>Identidad humana global, sin credenciales (D19, D21).</summary>
public class Persona
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; }
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public TipoDocumentoIdentidad? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Telefono { get; set; }
    public DateTime CreadoEn { get; set; }

    /// <summary>Calculado al mostrar; no se guarda en la base (RN-62).</summary>
    public string NombreCompleto => string.Join(' ',
        new[] { PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido }
            .Where(p => !string.IsNullOrWhiteSpace(p)));

    public Usuario? Usuario { get; set; }
    public ICollection<VinculoEmpresa> Vinculos { get; set; } = new List<VinculoEmpresa>();
}
