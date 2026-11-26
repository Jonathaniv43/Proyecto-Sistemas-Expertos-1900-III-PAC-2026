using Norvia.Application.Features.Plan.ListarResponsables.DTOs;

namespace Norvia.Application.Features.Plan.ListarResponsables;

public interface IListarResponsablesRepositorio
{
    /// <summary>Vínculos activos de la empresa de la sesión con un rol que puede tener tareas, por nombre.</summary>
    Task<IReadOnlyList<ResponsableResponseDto>> ListarAsync(CancellationToken ct);
}
