using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Application.Features.Plan.ListarTareas.DTOs;

namespace Norvia.Application.Features.Plan.ListarTareas;

/// <summary>
/// HU-042, HU-046: las tareas del plan de la empresa, empezando por las de prioridad alta y los riesgos más altos,
/// filtradas por estado, vencidas, responsable, origen, prioridad y fecha límite. "Vencida" no es un estado guardado:
/// pendiente o en progreso con la fecha límite antes de hoy en Honduras.
/// </summary>
public sealed class ListarTareasServicio(IListarTareasRepositorio repositorio, IReloj reloj) : IServicioCasoDeUso
{
    public async Task<IReadOnlyList<TareaResponseDto>> EjecutarAsync(FiltrosTareasDto filtros, CancellationToken ct) =>
        await repositorio.ListarAsync(filtros, Asignacion.HoyEnHonduras(await reloj.AhoraAsync(ct)), ct);
}
