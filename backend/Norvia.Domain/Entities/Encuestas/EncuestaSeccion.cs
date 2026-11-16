namespace Norvia.Domain.Entities.Encuestas;

/// <summary>Bloque/título de sección dentro de una versión de encuesta (estilo Forms).</summary>
public class EncuestaSeccion : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public Guid EncuestaVersionId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public short Orden { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public EncuestaVersion EncuestaVersion { get; set; } = null!;
    public ICollection<EncuestaPregunta> Preguntas { get; set; } = new List<EncuestaPregunta>();
}
