using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Empresas.ListarRubrosRegistro;
using Norvia.Application.Features.Empresas.ListarRubrosRegistro.DTOs;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Empresas;

// Rubro es una tabla global (sin EmpresaId): no le aplica el filtro por empresa.
public sealed class ListarRubrosRegistroRepositorio(NorviaDbContext db) : IListarRubrosRegistroRepositorio
{
    public Task<List<RubroRegistroResponseDto>> ListarActivosAsync(CancellationToken cancellationToken) =>
        db.Rubro.AsNoTracking()
            .Where(r => r.Activo)
            .OrderBy(r => r.Nombre)
            .Select(r => new RubroRegistroResponseDto(r.Codigo, r.Nombre))
            .ToListAsync(cancellationToken);
}
