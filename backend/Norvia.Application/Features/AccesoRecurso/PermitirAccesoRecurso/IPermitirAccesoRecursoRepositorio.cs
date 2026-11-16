using Norvia.Domain.Enums;

namespace Norvia.Application.Features.AccesoRecurso.PermitirAccesoRecurso;

public sealed record PermisoRegistrado(Guid Id, AlcancePermiso Alcance);

public interface IPermitirAccesoRecursoRepositorio
{
    Task<PermisoRegistrado?> BuscarPermisoAsync(string metodo, string ruta, CancellationToken ct);
    Task<bool> VinculoTienePermisoAsync(Guid vinculoId, Guid permisoId, CancellationToken ct);
    Task<bool> StaffTienePermisoAsync(Guid accesoId, Guid permisoId, CancellationToken ct);
}
