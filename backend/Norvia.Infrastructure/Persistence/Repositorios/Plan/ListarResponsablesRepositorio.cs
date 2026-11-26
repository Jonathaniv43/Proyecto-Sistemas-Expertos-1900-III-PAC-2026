using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Plan.ListarResponsables;
using Norvia.Application.Features.Plan.ListarResponsables.DTOs;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

public sealed class ListarResponsablesRepositorio(NorviaDbContext db) : IListarResponsablesRepositorio
{
    public async Task<IReadOnlyList<ResponsableResponseDto>> ListarAsync(CancellationToken ct)
    {
        var filas = await ConsultaResponsables.Validos(db)
            .Select(v => new { v.Id, v.Persona, v.Rol.Codigo })
            .ToListAsync(ct);
        return filas.Select(f => new ResponsableResponseDto(f.Id, f.Persona.NombreCompleto, f.Codigo))
            .OrderBy(r => r.NombreCompleto, StringComparer.CurrentCulture)
            .ToList();
    }
}
