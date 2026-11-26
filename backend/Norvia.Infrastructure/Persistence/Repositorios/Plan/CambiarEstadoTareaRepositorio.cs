using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Plan.CambiarEstadoTarea;
using Norvia.Application.Features.Plan.DTOs;
using Norvia.Domain.Entities.Plan;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

// Tarea y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
public sealed class CambiarEstadoTareaRepositorio(NorviaDbContext db) : ICambiarEstadoTareaRepositorio
{
    public Task<Tarea?> BuscarAsync(Guid tareaId, CancellationToken ct) => db.Tarea.FirstOrDefaultAsync(t => t.Id == tareaId, ct);

    public Task<short> RolDelVinculoAsync(Guid vinculoId, CancellationToken ct) =>
        db.VinculoEmpresa.AsNoTracking().Where(v => v.Id == vinculoId).Select(v => v.RolId).SingleAsync(ct);

    // La tabla Archivo (evidencias) llega con HU-045: por ahora ninguna tarea tiene evidencia.
    public Task<int> EvidenciasAsync(Guid tareaId, CancellationToken ct) => Task.FromResult(0);

    public async Task<TareaResponseDto> ProyectarAsync(Guid tareaId, CancellationToken ct) =>
        (await ProyeccionTarea.ListarAsync(db.Tarea.Where(t => t.Id == tareaId), ct)).Single();
}
