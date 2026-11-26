using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.AccesoRecurso.PermitirAccesoRecurso;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.AccesoRecurso;

public sealed class PermitirAccesoRecursoRepositorio(NorviaDbContext db) : IPermitirAccesoRecursoRepositorio
{
    public Task<PermisoRegistrado?> BuscarPermisoAsync(string metodo, string ruta, CancellationToken ct) =>
        db.Permiso.AsNoTracking()
            .Where(p => p.Activo && p.MetodoHttp == metodo && p.RutaPatron == ruta)
            .Select(p => new PermisoRegistrado(p.Id, p.Alcance))
            .FirstOrDefaultAsync(ct);

    public Task<bool> VinculoTienePermisoAsync(Guid vinculoId, Guid permisoId, CancellationToken ct) =>
        db.VinculoEmpresa // el filtro global de tenant ya limita a la empresa de la sesión
            .Where(v => v.Id == vinculoId
                     && v.Estado == EstadoVinculo.Activo
                     && v.Empresa.Estado == EstadoEmpresa.Activa)
            .AnyAsync(v => v.Rol.Permisos.Any(rp => rp.PermisoId == permisoId), ct);

    public Task<bool> StaffTienePermisoAsync(Guid accesoId, Guid permisoId, CancellationToken ct) =>
        db.StaffPlataforma
            .Where(s => s.AccesoId == accesoId && s.Activo)
            .AnyAsync(s => s.RolStaffPlataforma.Permisos.Any(rp => rp.PermisoId == permisoId), ct);
}
