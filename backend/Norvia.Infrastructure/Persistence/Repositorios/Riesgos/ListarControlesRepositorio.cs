using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ListarControles;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa y Control son tablas de empresa: el filtro global limita a la empresa de la sesión.
public sealed class ListarControlesRepositorio(NorviaDbContext db) : IListarControlesRepositorio
{
    public Task<bool> ExisteRiesgoAsync(Guid riesgoId, CancellationToken ct) =>
        db.RiesgoEmpresa.AnyAsync(r => r.Id == riesgoId, ct);

    public async Task<IReadOnlyList<ControlResponseDto>> ListarAsync(Guid riesgoId, CancellationToken ct)
    {
        var controles = await db.Control.AsNoTracking().Where(c => c.RiesgoEmpresaId == riesgoId).ToListAsync(ct);
        // El tipo se guarda como texto: la jerarquía (orden del enum) se aplica en memoria.
        return controles.OrderBy(c => c.Tipo).ThenBy(c => c.CreadoEn).ThenBy(c => c.Id).Select(ControlResponseDto.De).ToList();
    }
}
