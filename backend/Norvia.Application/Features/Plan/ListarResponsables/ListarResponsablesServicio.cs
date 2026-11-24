using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Plan.ListarResponsables.DTOs;

namespace Norvia.Application.Features.Plan.ListarResponsables;

/// <summary>HU-043: a quién se le puede asignar una tarea (vínculos activos de la empresa, sin encuestados).</summary>
public sealed class ListarResponsablesServicio(IListarResponsablesRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<IReadOnlyList<ResponsableResponseDto>> EjecutarAsync(CancellationToken ct) => repositorio.ListarAsync(ct);
}
