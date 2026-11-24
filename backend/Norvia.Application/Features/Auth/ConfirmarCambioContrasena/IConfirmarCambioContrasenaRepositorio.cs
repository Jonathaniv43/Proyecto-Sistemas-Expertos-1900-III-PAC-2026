using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.ConfirmarCambioContrasena;

public interface IConfirmarCambioContrasenaRepositorio
{
    Task<Acceso?> BuscarAccesoAsync(Guid accesoId, CancellationToken cancellationToken);

    /// <summary>Marca para borrar todas las sesiones del Acceso, en todas sus empresas.</summary>
    Task EliminarRefreshDelAccesoAsync(Guid accesoId, CancellationToken cancellationToken);
}
