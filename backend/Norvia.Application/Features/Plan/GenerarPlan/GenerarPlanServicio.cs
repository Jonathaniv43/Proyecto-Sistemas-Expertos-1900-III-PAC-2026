using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Plan.GenerarPlan.DTOs;
using Norvia.Application.Features.Plan.Propuesta;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Plan.GenerarPlan;

/// <summary>
/// HU-042: confirma el plan revisado. Vuelve a calcular la propuesta (no confía en lo que mande el cliente) y crea
/// las tareas elegidas, pendientes y sin responsable (eso es HU-043).
/// <list type="bullet">
/// <item>Cada tarea conserva su origen: el riesgo y, si lo implementa, el control (RN-23, D6).</item>
/// <item>Generarlo dos veces no duplica: lo que ya tiene tarea no se propone, y la base lo garantiza con
/// los índices únicos por plantilla de cada riesgo y por control.</item>
/// </list>
/// </summary>
public sealed class GenerarPlanServicio(IGenerarPlanRepositorio repositorio, IUnidadDeTrabajo unidadDeTrabajo, ITenantActual sesion)
    : IServicioCasoDeUso
{
    public async Task<Resultado<GenerarPlanResponseDto>> EjecutarAsync(GenerarPlanRequestDto request, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;

        var claves = request.Claves!.ToHashSet(StringComparer.Ordinal);
        var datos = await repositorio.DatosAsync(ct);
        var tareas = PropuestaPlan.Calcular(datos.Riesgos)
            .Where(t => claves.Contains(t.Clave))
            .Select(t => new Tarea
            {
                Titulo = t.Titulo,
                Descripcion = t.Descripcion,
                Estado = EstadoTarea.Pendiente,
                Prioridad = t.Prioridad,
                RequiereEvidencia = t.RequiereEvidencia,
                RiesgoEmpresaId = t.RiesgoId,
                ControlId = t.ControlId,
                PlantillaTareaId = t.PlantillaId,
                CreadaPorId = vinculoId
            })
            .ToList();

        if (tareas.Count > 0)
        {
            repositorio.Agregar(tareas);
            await unidadDeTrabajo.GuardarCambiosAsync(ct);
        }
        return new GenerarPlanResponseDto(tareas.Count);
    }
}
