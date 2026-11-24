using Norvia.Application.Features.Plan.DTOs;
using Norvia.Application.Features.Plan.ListarTareas.DTOs;

namespace Norvia.Application.Features.Plan.ListarTareas;

public interface IListarTareasRepositorio
{
    /// <summary>
    /// Tareas de la empresa de la sesión que cumplen los filtros (<paramref name="hoy"/> decide cuáles están vencidas),
    /// por prioridad, nivel del riesgo (más alto primero) y, dentro de cada riesgo, controles por efectividad y luego tareas modelo.
    /// </summary>
    Task<IReadOnlyList<TareaResponseDto>> ListarAsync(FiltrosTareasDto filtros, DateOnly hoy, CancellationToken ct);
}
