namespace Norvia.Domain.Entities.Encuestas;

/// <summary>Copia editable de una pregunta dentro de una versión de encuesta (RN-12, D26).</summary>
public class EncuestaPregunta : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid EncuestaVersionId { get; set; }
    public Guid? EncuestaSeccionId { get; set; }
    public Guid? CategoriaRiesgoId { get; set; }
    public Guid? PreguntaExploratoriaId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public Guid TipoRespuestaPreguntaId { get; set; }
    public bool Obligatoria { get; set; } = true;
    public Guid? PeligroId { get; set; }
    public short Orden { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public EncuestaVersion EncuestaVersion { get; set; } = null!;
    public EncuestaSeccion? EncuestaSeccion { get; set; }
    public CategoriaRiesgo? CategoriaRiesgo { get; set; }
    public PreguntaExploratoria? PreguntaExploratoria { get; set; }
    public TipoRespuestaPreguntaCatalogo TipoRespuestaPregunta { get; set; } = null!;
    public Peligro? Peligro { get; set; }
    public ICollection<EncuestaOpcion> Opciones { get; set; } = new List<EncuestaOpcion>();
}
