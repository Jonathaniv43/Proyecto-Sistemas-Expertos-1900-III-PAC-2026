using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Plan.Propuesta;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

/// <summary>
/// Compartido por los repositorios del plan que necesitan la propuesta (vista previa y confirmación).
/// RiesgoEmpresa, EvaluacionRiesgo, Control y Tarea son tablas de empresa (filtro global); PlantillaTarea es catálogo global.
/// </summary>
internal static class ConsultaPropuestaPlan
{
    internal static async Task<DatosPropuestaPlan> DatosAsync(NorviaDbContext db, CancellationToken ct)
    {
        var riesgos = await db.RiesgoEmpresa.AsNoTracking()
            .Where(r => r.Estado == EstadoRiesgo.Confirmado)
            .Select(r => new
            {
                r.Id,
                Nombre = r.Peligro != null ? r.Peligro.Nombre : r.DescripcionPeligro,
                r.PeligroId,
                r.CreadoEn,
                Evaluacion = r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                    .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                    .Select(e => new { e.Probabilidad, e.Severidad })
                    .FirstOrDefault(),
                // Controles propuestos que todavía no tienen tarea.
                Controles = r.Controles
                    .Where(c => c.Estado == EstadoControl.Propuesto && !db.Tarea.Any(t => t.ControlId == c.Id))
                    .Select(c => new ControlParaPlan(c.Id, c.Tipo, c.Descripcion, c.CreadoEn, c.PlantillaTareaId,
                        c.PlantillaTarea != null ? c.PlantillaTarea.Titulo : null,
                        c.PlantillaTarea != null && c.PlantillaTarea.RequiereEvidencia))
                    .ToList()
            })
            .ToListAsync(ct);

        var evaluados = riesgos.Where(r => r.Evaluacion is not null).ToList();
        var peligros = evaluados.Where(r => r.PeligroId is not null).Select(r => r.PeligroId!.Value).Distinct().ToList();
        var ids = evaluados.Select(r => r.Id).ToList();

        // Tareas modelo: plantillas activas del peligro sin tipo de control.
        var plantillas = await db.PlantillaTarea.AsNoTracking()
            .Where(p => p.Activa && p.TipoControl == null && p.PeligroId != null && peligros.Contains(p.PeligroId.Value))
            .Select(p => new { PeligroId = p.PeligroId!.Value, Plantilla = new PlantillaParaPlan(p.Id, p.Titulo, p.Descripcion, p.PrioridadSugerida, p.RequiereEvidencia) })
            .ToListAsync(ct);
        var conTarea = (await db.Tarea.AsNoTracking()
                .Where(t => t.RiesgoEmpresaId != null && t.PlantillaTareaId != null && ids.Contains(t.RiesgoEmpresaId.Value))
                .Select(t => new { Riesgo = t.RiesgoEmpresaId!.Value, Plantilla = t.PlantillaTareaId!.Value })
                .ToListAsync(ct))
            .ToHashSet();

        var lista = evaluados.Select(r => new RiesgoParaPlan(
                r.Id, r.Nombre ?? "Peligro sin nombre", r.Evaluacion!.Probabilidad, r.Evaluacion.Severidad, r.CreadoEn, r.Controles,
                plantillas.Where(p => p.PeligroId == r.PeligroId && !conTarea.Contains(new { Riesgo = r.Id, Plantilla = p.Plantilla.Id }))
                    .Select(p => p.Plantilla).ToList()))
            .ToList();
        return new DatosPropuestaPlan(lista, riesgos.Count - evaluados.Count);
    }
}
