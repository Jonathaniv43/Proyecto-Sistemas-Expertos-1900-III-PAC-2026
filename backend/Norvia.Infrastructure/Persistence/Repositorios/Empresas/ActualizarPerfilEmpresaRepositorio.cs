using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Empresas.ActualizarPerfilEmpresa;
using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Empresas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Empresas;

// Empresa es tabla raíz: sin HasQueryFilter; el caller pasa el Id de ITenantActual (RN-06).
public sealed class ActualizarPerfilEmpresaRepositorio(NorviaDbContext db) : IActualizarPerfilEmpresaRepositorio
{
    public Task<Empresa?> BuscarAsync(Guid empresaId, CancellationToken ct) =>
        db.Empresa
            .Include(e => e.Rubro)
            .FirstOrDefaultAsync(e => e.Id == empresaId, ct);

    public Task<short?> BuscarRubroIdActivoAsync(string codigo, CancellationToken ct) =>
        db.Rubro
            .Where(r => r.Codigo == codigo && r.Activo)
            .Select(r => (short?)r.Id)
            .FirstOrDefaultAsync(ct);

    public Task<Rubro?> BuscarRubroAsync(short rubroId, CancellationToken ct) =>
        db.Rubro.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rubroId, ct);
}
