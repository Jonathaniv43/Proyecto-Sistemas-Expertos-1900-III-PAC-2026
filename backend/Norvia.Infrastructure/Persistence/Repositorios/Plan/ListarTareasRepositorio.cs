using Norvia.Application.Features.Plan.DTOs;
using Norvia.Application.Features.Plan.ListarTareas;
using Norvia.Application.Features.Plan.ListarTareas.DTOs;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

// Tarea es tabla de empresa: el filtro global limita a la empresa de la sesión.
public sealed class ListarTareasRepositorio(NorviaDbContext db) : IListarTareasRepositorio
{
    public async Task<IReadOnlyList<TareaResponseDto>> ListarAsync(FiltrosTareasDto f, DateOnly hoy, CancellationToken ct)
    {
        var q = db.Tarea.AsQueryable();

        if (f.Estado is { } estado) q = q.Where(t => t.Estado == estado);
        if (f.Vencidas == true)
            q = q.Where(t => (t.Estado == EstadoTarea.Pendiente || t.Estado == EstadoTarea.EnProgreso) && t.FechaLimite < hoy);
        if (f.ResponsableId is { } responsable) q = q.Where(t => t.ResponsableId == responsable);
        if (f.SinResponsable == true) q = q.Where(t => t.ResponsableId == null);
        q = f.Origen switch
        {
            OrigenTarea.Riesgo => q.Where(t => t.RiesgoEmpresaId != null),
            OrigenTarea.Accidente => q.Where(t => t.AccidenteId != null),
            OrigenTarea.Manual => q.Where(t => t.RiesgoEmpresaId == null && t.AccidenteId == null),
            _ => q
        };
        if (f.Prioridad is { } prioridad) q = q.Where(t => t.Prioridad == prioridad);
        if (f.FechaDesde is { } desde) q = q.Where(t => t.FechaLimite >= desde);
        if (f.FechaHasta is { } hasta) q = q.Where(t => t.FechaLimite <= hasta);

        return await ProyeccionTarea.ListarAsync(q, ct);
    }
}
