using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.CerrarSesion;

public interface ICerrarSesionRepositorio
{
    Task<RefreshToken?> BuscarRefreshAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>Marca la fila para borrar; no guarda.</summary>
    void EliminarRefresh(RefreshToken refresh);
}
