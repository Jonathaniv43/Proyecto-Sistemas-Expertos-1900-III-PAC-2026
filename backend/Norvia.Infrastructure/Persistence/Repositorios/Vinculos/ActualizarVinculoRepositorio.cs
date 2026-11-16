using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Vinculos.ActualizarVinculo;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Vinculos;

// El filtro global limita a la empresa de la sesión.
public sealed class ActualizarVinculoRepositorio(NorviaDbContext db) : IActualizarVinculoRepositorio
{
    public Task<VinculoEmpresa?> BuscarAsync(Guid id, CancellationToken ct) =>
        db.VinculoEmpresa
            .Include(v => v.Persona).Include(v => v.Acceso).Include(v => v.Rol)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Rol?> BuscarRolAsync(string codigo, CancellationToken ct) =>
        db.Rol.FirstOrDefaultAsync(r => r.Codigo == codigo, ct);

    public Task<int> ContarOtrosAdministradoresActivosAsync(Guid excepto, CancellationToken ct) =>
        db.VinculoEmpresa.CountAsync(v => v.Id != excepto
                                          && v.RolId == Rol.AdminEmpresaId
                                          && v.Estado == EstadoVinculo.Activo, ct);

    public void AgregarHistorial(HistorialEstadoVinculo historial) => db.HistorialEstadoVinculo.Add(historial);

    // RefreshToken es global (sin filtro); se limita por el vínculo, que ya es de esta empresa.
    public async Task EliminarRefreshDelVinculoAsync(Guid vinculoId, CancellationToken ct) =>
        db.RefreshToken.RemoveRange(
            await db.RefreshToken.Where(t => t.VinculoEmpresaId == vinculoId).ToListAsync(ct));
}
