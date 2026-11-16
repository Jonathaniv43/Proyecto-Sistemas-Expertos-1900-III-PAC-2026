namespace Norvia.Domain.Entities.Encuestas;

/// <summary>Versión editable o publicada de una encuesta (D8, RN-12).</summary>
public class EncuestaVersion : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid EncuestaId { get; set; }
    public int NumeroVersion { get; set; }
    public string? Instrucciones { get; set; }
    public EstadoVersionEncuesta Estado { get; set; } = EstadoVersionEncuesta.Borrador;
    public Guid CreadaPorId { get; set; }
    public DateTime CreadaEn { get; set; }
    public DateTime? PublicadaEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Encuesta Encuesta { get; set; } = null!;
    public VinculoEmpresa CreadaPor { get; set; } = null!;
    public ICollection<EncuestaCategoria> Categorias { get; set; } = new List<EncuestaCategoria>();
    public ICollection<EncuestaPregunta> Preguntas { get; set; } = new List<EncuestaPregunta>();
}
