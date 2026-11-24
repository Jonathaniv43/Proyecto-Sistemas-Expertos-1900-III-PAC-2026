using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Plan.AsignarTarea.DTOs;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.AsignarTarea;

/// <summary>
/// HU-043: asigna (o reasigna) el responsable y la fecha límite de una tarea.
/// <list type="bullet">
/// <item>Responsable: vínculo activo de la misma empresa (RN-07) que trabaje con el plan, no un encuestado.</item>
/// <item>La fecha límite es obligatoria (RN-22) y no puede ser anterior a hoy, salvo que se conserve la que ya tenía
/// (design-system § Campo de fecha: al editar se conserva aunque esté vencida).</item>
/// <item>Las tareas completadas o canceladas no se reasignan. El estado no cambia: iniciarla es HU-044.</item>
/// </list>
/// Avisar al responsable (RN-77) es HU-081.
/// </summary>
public sealed class AsignarTareaServicio(IAsignarTareaRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo, IReloj reloj)
    : IServicioCasoDeUso
{
    public async Task<Resultado<TareaResponseDto>> EjecutarAsync(Guid tareaId, AsignarTareaRequestDto request, CancellationToken ct)
    {
        if (await repositorio.BuscarAsync(tareaId, ct) is not { } tarea) return Errores.TareaNoEncontrada;
        if (tarea.Estado is EstadoTarea.Completada or EstadoTarea.Cancelada) return Errores.TareaCerrada;

        var fecha = request.FechaLimite!.Value;
        if (fecha != tarea.FechaLimite && fecha < Asignacion.HoyEnHonduras(await reloj.AhoraAsync(ct))) return Errores.FechaPasada;
        var responsable = request.ResponsableId!.Value;
        if (!await repositorio.EsResponsableValidoAsync(responsable, ct)) return Errores.ResponsableInvalido;

        tarea.ResponsableId = responsable;
        tarea.FechaLimite = fecha;
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await repositorio.ProyectarAsync(tarea.Id, ct);
    }
}
