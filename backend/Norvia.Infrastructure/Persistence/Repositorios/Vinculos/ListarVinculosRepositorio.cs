using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Vinculos.ListarVinculos;
using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Vinculos;

// El filtro global limita a la empresa de la sesión.
public sealed class ListarVinculosRepositorio(NorviaDbContext db) : IListarVinculosRepositorio
{
    public Task<List<VinculoEmpresa>> ListarAsync(EstadoVinculo? estado, string? rolCodigo, CancellationToken ct) =>
        db.VinculoEmpresa.AsNoTracking()
            .Include(v => v.Persona).Include(v => v.Acceso).Include(v => v.Rol)
            .Where(v => estado == null || v.Estado == estado)
            .Where(v => rolCodigo == null || v.Rol.Codigo == rolCodigo)
            .OrderBy(v => v.Persona.PrimerApellido).ThenBy(v => v.Persona.PrimerNombre)
            .ToListAsync(ct);
}
