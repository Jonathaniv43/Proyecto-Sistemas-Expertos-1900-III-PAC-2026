using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.RefrescarSesion;

public interface IRefrescarSesionRepositorio
{
    /// <remarks>
    /// Incluye <c>Acceso.Staff.RolStaffPlataforma</c>, <c>VinculoEmpresa.Rol</c> y <c>VinculoEmpresa.Empresa</c>.
    /// </remarks>
    Task<RefreshToken?> BuscarRefreshAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>true si la persona puede usar la plataforma (<c>Usuario.Activo</c>): una cuenta desactivada pierde la sesión al renovar.</summary>
    Task<bool> UsuarioActivoAsync(Guid personaId, CancellationToken cancellationToken);

    /// <summary>Marca la fila para borrar; no guarda.</summary>
    void EliminarRefresh(RefreshToken refresh);

    /// <summary>Deja la fila pendiente; no guarda.</summary>
    void AgregarRefresh(RefreshToken refresh);
}
