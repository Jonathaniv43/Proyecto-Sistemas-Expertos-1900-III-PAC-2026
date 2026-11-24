using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.ListarMisTareas;

/// <summary>
/// HU-044: las tareas asignadas a quien tiene la sesión (su vínculo, del token). Primero las abiertas (en progreso,
/// luego pendientes) por fecha límite más próxima y prioridad; después las completadas y canceladas, las más recientes primero.
/// </summary>
public sealed class ListarMisTareasServicio(IListarMisTareasRepositorio repositorio, ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<TareaResponseDto>>> EjecutarAsync(CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;

        var tareas = await repositorio.ListarAsync(vinculoId, ct);
        static int Grupo(EstadoTarea e) => e switch
        {
            EstadoTarea.EnProgreso => 0,
            EstadoTarea.Pendiente => 1,
            _ => 2
        };
        return Resultado<IReadOnlyList<TareaResponseDto>>.Exito(tareas
            .OrderBy(t => Grupo(t.Estado))
            .ThenBy(t => Grupo(t.Estado) == 2 ? 0 : t.FechaLimite?.DayNumber ?? int.MaxValue)
            .ThenBy(t => Grupo(t.Estado) == 2 ? 0 : t.Prioridad)
            .ThenByDescending(t => t.CompletadaEn ?? t.CreadaEn)
            .ThenBy(t => t.Id)
            .ToList());
    }
}
