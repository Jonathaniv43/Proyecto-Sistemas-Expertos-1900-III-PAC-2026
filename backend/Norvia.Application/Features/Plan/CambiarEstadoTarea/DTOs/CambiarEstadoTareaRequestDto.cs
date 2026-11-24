using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.CambiarEstadoTarea.DTOs;

/// <summary>Nuevo estado de una tarea (HU-044): <c>pendiente</c>, <c>en_progreso</c> o <c>completada</c>.</summary>
public sealed record CambiarEstadoTareaRequestDto
{
    public EstadoTarea? Estado { get; init; }
}
