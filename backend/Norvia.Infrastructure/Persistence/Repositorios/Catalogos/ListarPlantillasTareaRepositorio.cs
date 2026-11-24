using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ListarPlantillasTarea;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tabla global (sin EmpresaId): no le aplica el filtro por empresa.
public sealed class ListarPlantillasTareaRepositorio(NorviaDbContext db) : IListarPlantillasTareaRepositorio
{
    public Task<List<PlantillaTarea>> ListarAsync(Guid? peligroId, CancellationToken ct) =>
        db.PlantillaTarea.AsNoTracking().Where(p => peligroId == null || p.PeligroId == peligroId).ToListAsync(ct);
}
