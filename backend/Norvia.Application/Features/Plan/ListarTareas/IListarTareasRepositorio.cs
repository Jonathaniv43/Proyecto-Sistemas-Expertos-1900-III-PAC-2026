using Norvia.Application.Features.Plan.DTOs;

namespace Norvia.Application.Features.Plan.ListarTareas;

public interface IListarTareasRepositorio
{
    /// <summary>Tareas de la empresa de la sesión, por prioridad, nivel del riesgo (más alto primero) y, dentro de cada riesgo, controles por efectividad y luego tareas modelo.</summary>
    Task<IReadOnlyList<TareaResponseDto>> ListarAsync(CancellationToken ct);
}
