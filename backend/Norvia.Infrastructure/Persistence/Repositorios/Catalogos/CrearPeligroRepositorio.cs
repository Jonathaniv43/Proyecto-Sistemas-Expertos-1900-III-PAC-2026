using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Catalogos.CrearPeligro;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Catalogos;

// Tablas globales (sin EmpresaId): no les aplica el filtro por empresa.
public sealed class CrearPeligroRepositorio(NorviaDbContext db) : ICrearPeligroRepositorio
{
    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct) =>
        db.Peligro.AnyAsync(p => p.Codigo == codigo, ct);

    public void Agregar(Peligro peligro) => db.Peligro.Add(peligro);
}
