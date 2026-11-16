using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Empresas.ObtenerPerfilEmpresa;
using Norvia.Domain.Entities.Empresas;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Empresas;

// Empresa es tabla raíz: sin HasQueryFilter; el caller pasa el Id de ITenantActual (RN-06).
public sealed class ObtenerPerfilEmpresaRepositorio(NorviaDbContext db) : IObtenerPerfilEmpresaRepositorio
{
    public Task<Empresa?> BuscarAsync(Guid empresaId, CancellationToken ct) =>
        db.Empresa.AsNoTracking()
            .Include(e => e.Rubro)
            .FirstOrDefaultAsync(e => e.Id == empresaId, ct);
}
