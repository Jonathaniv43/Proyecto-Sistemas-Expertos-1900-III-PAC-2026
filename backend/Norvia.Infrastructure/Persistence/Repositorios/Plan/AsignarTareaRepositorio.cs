using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Plan.AsignarTarea;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Plan;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

// Tarea y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
public sealed class AsignarTareaRepositorio(NorviaDbContext db) : IAsignarTareaRepositorio
{
    public Task<Tarea?> BuscarAsync(Guid tareaId, CancellationToken ct) => db.Tarea.FirstOrDefaultAsync(t => t.Id == tareaId, ct);

    public Task<bool> EsResponsableValidoAsync(Guid vinculoId, CancellationToken ct) => ConsultaResponsables.EsValidoAsync(db, vinculoId, ct);

    public async Task<TareaResponseDto> ProyectarAsync(Guid tareaId, CancellationToken ct) =>
        (await ProyeccionTarea.ListarAsync(db.Tarea.Where(t => t.Id == tareaId), ct)).Single();
}
