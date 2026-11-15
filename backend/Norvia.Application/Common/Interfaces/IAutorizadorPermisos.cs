using Norvia.Domain.Enums;

namespace Norvia.Application.Common.Interfaces;

public sealed record PermisoRegistrado(Guid Id, AlcancePermiso Alcance);

public interface IAutorizadorPermisos
{
    Task<PermisoRegistrado?> BuscarPermisoAsync(string metodo, string ruta, CancellationToken ct);
    Task<bool> VinculoTienePermisoAsync(Guid vinculoId, Guid permisoId, CancellationToken ct);
    Task<bool> StaffTienePermisoAsync(Guid accesoId, Guid permisoId, CancellationToken ct);
}