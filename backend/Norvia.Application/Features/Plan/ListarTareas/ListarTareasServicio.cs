using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Plan.ListarTareas.DTOs;

namespace Norvia.Application.Features.Plan.ListarTareas;

/// <summary>HU-042: las tareas del plan de la empresa, empezando por las de prioridad alta y los riesgos más altos.</summary>
public sealed class ListarTareasServicio(IListarTareasRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<IReadOnlyList<TareaResponseDto>> EjecutarAsync(CancellationToken ct) => repositorio.ListarAsync(ct);
}
