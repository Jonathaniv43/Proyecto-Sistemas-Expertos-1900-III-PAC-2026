using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.Interfaces;

/// <summary>Persistencia de auth. Sin reglas de negocio.</summary>
public interface IAuthRepositorio
{
    Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default);

    Task<VinculoEmpresa?> BuscarVinculoConEmpresaYRolAsync(
        Guid accesoId,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<StaffPlataforma?> BuscarStaffConRolAsync(Guid accesoId, CancellationToken cancellationToken = default);

    Task<RefreshToken?> BuscarRefreshConSesionAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task EliminarRefreshAsync(RefreshToken refresh, CancellationToken cancellationToken = default);

    Task AgregarRefreshAsync(RefreshToken refresh, CancellationToken cancellationToken = default);
}
