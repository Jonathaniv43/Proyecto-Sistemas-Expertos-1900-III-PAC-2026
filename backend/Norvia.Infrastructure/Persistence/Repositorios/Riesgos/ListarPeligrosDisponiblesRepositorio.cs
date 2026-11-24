using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles;
using Norvia.Application.Features.Riesgos.ListarPeligrosDisponibles.DTOs;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// Peligro es un catálogo global (sin EmpresaId).
public sealed class ListarPeligrosDisponiblesRepositorio(NorviaDbContext db) : IListarPeligrosDisponiblesRepositorio
{
    public async Task<IReadOnlyList<PeligroDisponibleResponseDto>> ListarAsync(CancellationToken ct) =>
        await db.Peligro.AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new PeligroDisponibleResponseDto(p.Id, p.Nombre, p.Descripcion, p.Clasificacion))
            .ToListAsync(ct);
}
