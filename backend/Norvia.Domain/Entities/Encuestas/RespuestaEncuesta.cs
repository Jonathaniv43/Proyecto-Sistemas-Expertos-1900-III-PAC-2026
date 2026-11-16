namespace Norvia.Domain.Entities.Encuestas;

/// <summary>
/// Respuesta anónima a una versión de encuesta: sin vínculo ni hora (D27, RN-72).
/// </summary>
public class RespuestaEncuesta : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid EncuestaVersionId { get; set; }
    /// <summary>Solo la fecha (sin hora), para no cruzar con <see cref="ParticipacionEncuesta.ParticipoEn"/>.</summary>
    public DateOnly FechaRespuesta { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public EncuestaVersion EncuestaVersion { get; set; } = null!;
    public ICollection<DetalleRespuesta> Detalles { get; set; } = new List<DetalleRespuesta>();
}
