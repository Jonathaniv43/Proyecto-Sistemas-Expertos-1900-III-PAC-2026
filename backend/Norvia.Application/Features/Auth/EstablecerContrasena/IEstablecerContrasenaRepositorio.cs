using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.EstablecerContrasena;

public interface IEstablecerContrasenaRepositorio
{
    Task<Acceso?> BuscarAccesoAsync(Guid accesoId, CancellationToken cancellationToken);

    /// <summary>Quita todos los refresh del Acceso: cierra sus sesiones en todas las empresas.</summary>
    Task EliminarRefreshDelAccesoAsync(Guid accesoId, CancellationToken cancellationToken);
}
