using Norvia.Application.Features.Plan.DTOs;
using Norvia.Application.Features.Plan.ListarMisTareas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

public sealed class ListarMisTareasRepositorio(NorviaDbContext db) : IListarMisTareasRepositorio
{
    public async Task<IReadOnlyList<TareaResponseDto>> ListarAsync(Guid responsableId, CancellationToken ct) =>
        await ProyeccionTarea.ListarAsync(db.Tarea.Where(t => t.ResponsableId == responsableId), ct);
}
