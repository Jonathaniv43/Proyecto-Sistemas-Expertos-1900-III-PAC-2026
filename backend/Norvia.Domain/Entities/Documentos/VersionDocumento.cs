namespace Norvia.Domain.Entities.Documentos;

/// <summary>
/// Una versión de un documento (D13). <see cref="Contenido"/> es Markdown con los marcadores ya reemplazados por los
/// datos de la empresa. Solo se edita mientras está en borrador (RN-50); aprobarla registra quién y cuándo.
/// </summary>
public class VersionDocumento : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid DocumentoId { get; set; }

    /// <summary>Desde 1, sin saltos, dentro de cada documento.</summary>
    public int NumeroVersion { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public EstadoVersionDocumento Estado { get; set; } = EstadoVersionDocumento.Borrador;
    public Guid CreadaPorId { get; set; }
    public DateTime CreadaEn { get; set; }
    public Guid? AprobadaPorId { get; set; }
    public DateTime? AprobadaEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Documento Documento { get; set; } = null!;
    public VinculoEmpresa CreadaPor { get; set; } = null!;
    public VinculoEmpresa? AprobadaPor { get; set; }
}
