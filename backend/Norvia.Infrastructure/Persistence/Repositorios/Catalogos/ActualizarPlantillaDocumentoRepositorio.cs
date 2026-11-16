using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ActualizarPlantillaDocumentoRepositorio(NorviaDbContext db) : IActualizarPlantillaDocumentoRepositorio
{
    public Task<PlantillaDocumento?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.PlantillaDocumento.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> RubroActivoExisteAsync(short rubroId, CancellationToken ct) =>
        db.Rubro.AnyAsync(r => r.Id == rubroId && r.Activo, ct);
}
