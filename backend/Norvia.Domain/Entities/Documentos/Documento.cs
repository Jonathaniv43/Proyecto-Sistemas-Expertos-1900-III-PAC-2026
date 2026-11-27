namespace Norvia.Domain.Entities.Documentos;

/// <summary>
/// Política o procedimiento de la empresa (M9, D13). Su contenido está en las versiones: cada una es un registro propio
/// y la aprobada nunca cambia (RN-50). Puede salir de una plantilla global (HU-070).
/// </summary>
public class Documento : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }

    /// <summary>Plantilla de origen; nulo si no salió de una.</summary>
    public Guid? PlantillaDocumentoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public TipoDocumento TipoDocumento { get; set; }
    public Guid CreadoPorId { get; set; }
    public DateTime CreadoEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public PlantillaDocumento? PlantillaDocumento { get; set; }
    public VinculoEmpresa CreadoPor { get; set; } = null!;
    public ICollection<VersionDocumento> Versiones { get; set; } = new List<VersionDocumento>();
}
