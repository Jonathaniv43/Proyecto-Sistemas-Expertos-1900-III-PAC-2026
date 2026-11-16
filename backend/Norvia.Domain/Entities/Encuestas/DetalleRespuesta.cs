namespace Norvia.Domain.Entities.Encuestas;

/// <summary>
/// Respuesta a una pregunta. En selección múltiple ambas columnas de valor son nulas
/// y las opciones van en <see cref="DetalleRespuestaOpcion"/> (RN-68, D26).
/// </summary>
public class DetalleRespuesta : ITieneEmpresa
{
    public Guid RespuestaEncuestaId { get; set; }
    public Guid EncuestaPreguntaId { get; set; }
    public Guid EmpresaId { get; set; }
    public Guid? EncuestaOpcionId { get; set; }
    public string? TextoLibre { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public RespuestaEncuesta RespuestaEncuesta { get; set; } = null!;
    public EncuestaPregunta EncuestaPregunta { get; set; } = null!;
    public EncuestaOpcion? EncuestaOpcion { get; set; }
    public ICollection<DetalleRespuestaOpcion> Opciones { get; set; } = new List<DetalleRespuestaOpcion>();
}
