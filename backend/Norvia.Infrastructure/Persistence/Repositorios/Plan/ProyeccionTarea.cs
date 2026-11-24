using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

/// <summary>
/// Compartido por los repositorios del plan que devuelven <see cref="TareaResponseDto"/>. Tarea, RiesgoEmpresa,
/// EvaluacionRiesgo, Control y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
/// </summary>
internal static class ProyeccionTarea
{
    /// <summary>
    /// Las tareas con su responsable, su riesgo (nombre y nivel inicial actual) y el tipo del control que implementan.
    /// Orden: prioridad, riesgo de nivel más alto y, dentro de cada riesgo, controles por efectividad (RN-21) y luego
    /// tareas modelo. Las confirmadas juntas comparten CreadaEn, así que no sirve para desempatar.
    /// </summary>
    internal static async Task<List<TareaResponseDto>> ListarAsync(IQueryable<Tarea> tareas, CancellationToken ct)
    {
        var filas = await tareas.AsNoTracking()
            .Select(t => new
            {
                t.Id, t.Titulo, t.Descripcion, t.Estado, t.Prioridad, t.ResponsableId,
                Responsable = t.Responsable != null ? t.Responsable.Persona : null,
                t.FechaLimite, t.RequiereEvidencia, t.RiesgoEmpresaId, t.AccidenteId,
                RiesgoNombre = t.RiesgoEmpresa != null
                    ? (t.RiesgoEmpresa.Peligro != null ? t.RiesgoEmpresa.Peligro.Nombre : t.RiesgoEmpresa.DescripcionPeligro)
                    : null,
                Evaluacion = t.RiesgoEmpresa != null
                    ? t.RiesgoEmpresa.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                        .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                        .Select(e => new { e.Probabilidad, e.Severidad })
                        .FirstOrDefault()
                    : null,
                ControlTipo = t.Control != null ? (TipoControl?)t.Control.Tipo : null,
                t.CreadaEn
            })
            .ToListAsync(ct);

        return filas
            .Select(f =>
            {
                int? nivel = f.Evaluacion is { } e ? e.Probabilidad * e.Severidad : null;
                var origen = f.RiesgoEmpresaId is not null ? OrigenTarea.Riesgo : f.AccidenteId is not null ? OrigenTarea.Accidente : OrigenTarea.Manual;
                return new TareaResponseDto(f.Id, f.Titulo, f.Descripcion, f.Estado, f.Prioridad, f.ResponsableId, f.Responsable?.NombreCompleto,
                    f.FechaLimite, f.RequiereEvidencia, origen, f.RiesgoEmpresaId, f.RiesgoNombre, nivel,
                    nivel is int n ? EvaluacionRiesgo.Clasificar(n) : null, f.ControlTipo, f.CreadaEn);
            })
            .OrderBy(t => t.Prioridad).ThenByDescending(t => t.RiesgoNivel ?? 0).ThenBy(t => t.RiesgoId)
            .ThenBy(t => t.ControlTipo is null).ThenBy(t => t.ControlTipo)
            .ThenBy(t => t.Titulo, StringComparer.CurrentCulture).ThenBy(t => t.CreadaEn).ThenBy(t => t.Id)
            .ToList();
    }
}
