using Norvia.Application.Features.Plan.DTOs;

namespace Norvia.Application.Features.Plan.ListarMisTareas;

public interface IListarMisTareasRepositorio
{
    /// <summary>Tareas de la empresa de la sesión cuyo responsable es el vínculo indicado.</summary>
    Task<IReadOnlyList<TareaResponseDto>> ListarAsync(Guid responsableId, CancellationToken ct);
}
