using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.CrearPlantillaTarea.DTOs;

/// <summary>Plantilla de tarea de un peligro. Sin <see cref="TipoControl"/> es una tarea modelo, no un control.</summary>
public sealed record CrearPlantillaTareaRequestDto
{
    public Guid? PeligroId { get; init; }
    public TipoControl? TipoControl { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Descripcion { get; init; }

    /// <summary>1 alta, 2 media, 3 baja.</summary>
    public short? PrioridadSugerida { get; init; }
    public bool RequiereEvidencia { get; init; }
}
