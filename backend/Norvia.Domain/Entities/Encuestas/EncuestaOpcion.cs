namespace Norvia.Domain.Entities.Encuestas;

/// <summary>Copia editable de una opción de respuesta dentro de una pregunta de encuesta (RN-12).</summary>
public class EncuestaOpcion : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid EncuestaPreguntaId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public short Orden { get; set; }
    public bool IndicaExposicion { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public EncuestaPregunta EncuestaPregunta { get; set; } = null!;
}
