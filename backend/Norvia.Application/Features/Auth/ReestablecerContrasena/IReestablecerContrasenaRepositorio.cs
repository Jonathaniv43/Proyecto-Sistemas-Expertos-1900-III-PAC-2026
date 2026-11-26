using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.ReestablecerContrasena;

public interface IReestablecerContrasenaRepositorio
{
    Task<Acceso?> BuscarAccesoAsync(Guid accesoId, CancellationToken cancellationToken);
    Task EliminarRefreshDelAccesoAsync(Guid accesoId, CancellationToken cancellationToken);
}