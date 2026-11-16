using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Vinculos.CrearVinculo;
using Norvia.Domain.Entities.Acceso;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Vinculos;

public sealed class CrearVinculoRepositorio(NorviaDbContext db) : ICrearVinculoRepositorio
{
    public Task<Rol?> BuscarRolAsync(string codigo, CancellationToken ct) =>
        db.Rol.FirstOrDefaultAsync(r => r.Codigo == codigo, ct);
}
