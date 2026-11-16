using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CrearPlantillaDocumento;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class CrearPlantillaDocumentoRepositorio(NorviaDbContext db) : ICrearPlantillaDocumentoRepositorio
{
    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct) =>
        db.PlantillaDocumento.AnyAsync(p => p.Codigo == codigo, ct);

    public Task<bool> RubroActivoExisteAsync(short rubroId, CancellationToken ct) =>
        db.Rubro.AnyAsync(r => r.Id == rubroId && r.Activo, ct);

    public void Agregar(PlantillaDocumento plantilla) => db.PlantillaDocumento.Add(plantilla);
}
