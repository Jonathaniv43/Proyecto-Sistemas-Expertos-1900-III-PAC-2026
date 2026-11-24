namespace Norvia.Application.Features.Eventos.CrearMedidaCorrectiva.DTOs;

/// <summary>
/// Medida correctiva (HU-050): se convierte en una tarea del plan con origen en el evento. Nace asignada, como una
/// tarea manual: responsable y fecha límite son obligatorios.
/// </summary>
public sealed record CrearMedidaCorrectivaRequestDto
{
    public string? Titulo { get; init; }
    public string? Descripcion { get; init; }

    /// <summary>1 alta, 2 media, 3 baja.</summary>
    public short? Prioridad { get; init; }
    public Guid? ResponsableId { get; init; }
    public DateOnly? FechaLimite { get; init; }
    public bool RequiereEvidencia { get; init; }
}
