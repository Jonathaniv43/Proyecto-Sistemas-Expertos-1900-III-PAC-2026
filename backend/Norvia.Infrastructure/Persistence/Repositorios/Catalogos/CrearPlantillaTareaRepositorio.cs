using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CrearPlantillaTarea;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class CrearPlantillaTareaRepositorio(NorviaDbContext db) : ICrearPlantillaTareaRepositorio
{
    public Task<bool> PeligroActivoAsync(Guid peligroId, CancellationToken ct) =>
        db.Peligro.AnyAsync(p => p.Id == peligroId && p.Activo, ct);

    public void Agregar(PlantillaTarea plantilla) => db.PlantillaTarea.Add(plantilla);
}
