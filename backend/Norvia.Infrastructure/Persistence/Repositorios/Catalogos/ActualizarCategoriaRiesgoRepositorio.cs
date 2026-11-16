using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ActualizarCategoriaRiesgoRepositorio(NorviaDbContext db) : IActualizarCategoriaRiesgoRepositorio
{
    public Task<CategoriaRiesgo?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.CategoriaRiesgo.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> RubroActivoExisteAsync(short rubroId, CancellationToken ct) =>
        db.Rubro.AnyAsync(r => r.Id == rubroId && r.Activo, ct);

    public Task<bool> ExisteNombreEnOtraAsync(Guid id, short rubroId, string nombre, CancellationToken ct) =>
        db.CategoriaRiesgo.AnyAsync(c => c.Id != id && c.RubroId == rubroId && c.Nombre == nombre, ct);
}
