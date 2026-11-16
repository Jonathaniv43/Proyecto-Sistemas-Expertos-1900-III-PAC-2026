using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CrearRubro;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class CrearRubroRepositorio(NorviaDbContext db) : ICrearRubroRepositorio
{
    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct) =>
        db.Rubro.AnyAsync(r => r.Codigo == codigo, ct);

    public Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct) =>
        db.Rubro.AnyAsync(r => r.Nombre == nombre, ct);

    public async Task<short> MayorIdAsync(CancellationToken ct) =>
        await db.Rubro.MaxAsync(r => (short?)r.Id, ct) ?? 0;

    public void Agregar(Rubro rubro) => db.Rubro.Add(rubro);
}
