using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.ActualizarRubro;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class ActualizarRubroRepositorio(NorviaDbContext db) : IActualizarRubroRepositorio
{
    public Task<Rubro?> BuscarAsync(short id, CancellationToken ct) =>
        db.Rubro.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> ExisteNombreEnOtroAsync(short id, string nombre, CancellationToken ct) =>
        db.Rubro.AnyAsync(r => r.Id != id && r.Nombre == nombre, ct);
}
