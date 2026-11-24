using Norvia.Application.Features.Plan.DTOs;
using Norvia.Application.Features.Plan.ListarTareas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

public sealed class ListarTareasRepositorio(NorviaDbContext db) : IListarTareasRepositorio
{
    public async Task<IReadOnlyList<TareaResponseDto>> ListarAsync(CancellationToken ct) => await ProyeccionTarea.ListarAsync(db.Tarea, ct);
}
