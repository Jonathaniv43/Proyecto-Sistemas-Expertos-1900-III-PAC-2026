namespace Norvia.Domain.Entities.Encuestas;

/// <summary>Categoría de riesgo seleccionada para una versión de encuesta (RN-11).</summary>
public class EncuestaCategoria : ITieneEmpresa
{
    public Guid EncuestaVersionId { get; set; }
    public Guid CategoriaRiesgoId { get; set; }
    public Guid EmpresaId { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public EncuestaVersion EncuestaVersion { get; set; } = null!;
    public CategoriaRiesgo CategoriaRiesgo { get; set; } = null!;
}
