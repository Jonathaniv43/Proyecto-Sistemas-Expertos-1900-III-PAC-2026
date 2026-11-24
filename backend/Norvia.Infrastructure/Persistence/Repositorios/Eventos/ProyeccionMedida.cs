using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.DTOs;
using Norvia.Domain.Entities.Plan;
using Norvia.Domain.Enums;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

/// <summary>Medidas correctivas como DTO. Tarea y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.</summary>
internal static class ProyeccionMedida
{
    internal static async Task<List<MedidaCorrectivaResponseDto>> ListarAsync(IQueryable<Tarea> tareas, CancellationToken ct)
    {
        var filas = await tareas.AsNoTracking()
            .Select(t => new
            {
                t.Id, t.Titulo, t.Descripcion, t.Estado, t.Prioridad, t.ResponsableId,
                Responsable = t.Responsable != null ? t.Responsable.Persona : null,
                t.FechaLimite, t.RequiereEvidencia, t.CreadaEn, t.CompletadaEn
            })
            .ToListAsync(ct);

        return filas
            .Select(f => new MedidaCorrectivaResponseDto(f.Id, f.Titulo, f.Descripcion, f.Estado, f.Prioridad, f.ResponsableId,
                f.Responsable?.NombreCompleto, f.FechaLimite, f.RequiereEvidencia, f.CreadaEn, f.CompletadaEn))
            .OrderBy(m => Orden(m.Estado)).ThenBy(m => m.FechaLimite ?? DateOnly.MaxValue).ThenBy(m => m.Prioridad)
            .ThenBy(m => m.CreadaEn).ThenBy(m => m.Id)
            .ToList();
    }

    private static int Orden(EstadoTarea estado) => estado switch
    {
        EstadoTarea.Pendiente or EstadoTarea.EnProgreso => 0,
        EstadoTarea.Completada => 1,
        _ => 2
    };
}
