namespace Norvia.Domain.Entities.Encuestas;

/// <summary>
/// Quién ya respondió una encuesta, sin relacionar con el contenido de la respuesta (D27, RN-71, RN-72).
/// </summary>
public class ParticipacionEncuesta : ITieneEmpresa
{
    public Guid EncuestaId { get; set; }
    public Guid VinculoEmpresaId { get; set; }
    public Guid EmpresaId { get; set; }
    public DateTime ParticipoEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public Encuesta Encuesta { get; set; } = null!;
    public VinculoEmpresa VinculoEmpresa { get; set; } = null!;
}
