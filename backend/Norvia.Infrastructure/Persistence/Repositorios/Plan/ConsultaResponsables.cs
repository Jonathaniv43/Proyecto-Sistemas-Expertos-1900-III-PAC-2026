using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Plan;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

/// <summary>Quién puede ser responsable de una tarea. VinculoEmpresa es tabla de empresa: el filtro global limita a la sesión.</summary>
internal static class ConsultaResponsables
{
    internal static IQueryable<Domain.Entities.Acceso.VinculoEmpresa> Validos(NorviaDbContext db) =>
        db.VinculoEmpresa.AsNoTracking()
            .Where(v => v.Estado == EstadoVinculo.Activo && Asignacion.RolesResponsables.Contains(v.RolId));

    internal static Task<bool> EsValidoAsync(NorviaDbContext db, Guid vinculoId, CancellationToken ct) =>
        Validos(db).AnyAsync(v => v.Id == vinculoId, ct);
}
