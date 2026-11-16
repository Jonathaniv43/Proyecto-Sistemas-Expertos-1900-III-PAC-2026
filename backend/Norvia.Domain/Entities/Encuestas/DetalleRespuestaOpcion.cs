namespace Norvia.Domain.Entities.Encuestas;

/// <summary>Opciones elegidas en una pregunta <c>seleccion_multiple</c> (D26, 1FN).</summary>
public class DetalleRespuestaOpcion : ITieneEmpresa
{
    public Guid RespuestaEncuestaId { get; set; }
    public Guid EncuestaPreguntaId { get; set; }
    public Guid EncuestaOpcionId { get; set; }
    public Guid EmpresaId { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public DetalleRespuesta DetalleRespuesta { get; set; } = null!;
    public EncuestaOpcion EncuestaOpcion { get; set; } = null!;
}
