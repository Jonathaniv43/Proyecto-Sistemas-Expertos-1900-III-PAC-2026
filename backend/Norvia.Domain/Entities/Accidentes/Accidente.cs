namespace Norvia.Domain.Entities.Accidentes;

/// <summary>
/// Evento de seguridad y salud en el trabajo reportado de forma anónima dentro de la empresa (D11, HU-047).
/// Por diseño no almacena quién lo reportó.
/// <para>
/// Pendiente de futuras HUs:
/// - HU-050: derivación a medidas correctivas (<c>Tarea</c>).
/// - HU-051: vinculación con <c>RiesgoEmpresa.AccidenteId</c>.
/// - HU-100: adjuntos de evidencia en <c>Archivo</c>.
/// </para>
/// </summary>
public class Accidente : ITieneEmpresa
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; set; }
    public TipoAccidente Tipo { get; set; }
    public EstadoEnfermedad? EstadoEnfermedad { get; set; }
    public bool EsGrave { get; set; }
    public DateTime FechaHoraEvento { get; set; }
    public string? Lugar { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime RegistradoEn { get; set; }
    public EstadoAccidente Estado { get; set; } = EstadoAccidente.Abierto;

    // Pendiente HU-049: columnas del modelo de datos reservadas para cierre e investigación/descarte.
    public DateTime? CerradoEn { get; set; }
    public Guid? DescartadoPorId { get; set; }
    public string? MotivoDescarte { get; set; }
    public DateTime? DescartadoEn { get; set; }

    public Empresa Empresa { get; set; } = null!;
    public VinculoEmpresa? DescartadoPor { get; set; }
    public InvestigacionAccidente? Investigacion { get; set; }
}
