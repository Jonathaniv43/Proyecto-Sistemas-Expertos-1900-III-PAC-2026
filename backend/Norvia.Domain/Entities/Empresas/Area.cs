namespace Norvia.Domain.Entities.Empresas;

/// <summary>Área de la empresa. Con registros asociados se desactiva, no se borra.</summary>
public class Area : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Guid? EncargadoId { get; set; }
    public bool Activa { get; set; } = true;

    public Empresa Empresa { get; set; } = null!;
    public VinculoEmpresa? Encargado { get; set; }
    public ICollection<RiesgoEmpresa> Riesgos { get; set; } = new List<RiesgoEmpresa>();
}
