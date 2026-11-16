namespace Norvia.Domain.Entities.Encuestas;

/// <summary>Encuesta propia de la empresa para diagnóstico de contexto (D8, HU-030).</summary>
public class Encuesta : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public EstadoEncuesta Estado { get; set; } = EstadoEncuesta.Borrador;
    public Guid CreadaPorId { get; set; }
    public DateTime CreadaEn { get; set; }
    public DateTime? CerradaEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public VinculoEmpresa CreadaPor { get; set; } = null!;
    public ICollection<EncuestaVersion> Versiones { get; set; } = new List<EncuestaVersion>();
}
