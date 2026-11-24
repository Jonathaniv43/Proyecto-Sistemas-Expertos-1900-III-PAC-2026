using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea.DTOs;

/// <summary>Edición parcial: lo que no se envía se conserva. El peligro no cambia; el estado va por /estado.</summary>
public sealed record ActualizarPlantillaTareaRequestDto
{
    public TipoControl? TipoControl { get; init; }

    /// <summary><c>true</c>: deja de ser un control y pasa a ser una tarea modelo.</summary>
    public bool? QuitarTipoControl { get; init; }

    public string? Titulo { get; init; }

    /// <summary>Texto vacío ("") borra la descripción.</summary>
    public string? Descripcion { get; init; }
    public short? PrioridadSugerida { get; init; }
    public bool? RequiereEvidencia { get; init; }
}
