using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tabla global (sin EmpresaId): no le aplica el filtro por empresa.
public sealed class ActualizarPlantillaTareaRepositorio(NorviaDbContext db) : IActualizarPlantillaTareaRepositorio
{
    public Task<PlantillaTarea?> BuscarAsync(Guid id, CancellationToken ct) => db.PlantillaTarea.FirstOrDefaultAsync(p => p.Id == id, ct);
}
