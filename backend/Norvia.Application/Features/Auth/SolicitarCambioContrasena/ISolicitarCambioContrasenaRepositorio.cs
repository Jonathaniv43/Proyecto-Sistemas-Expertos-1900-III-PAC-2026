using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.SolicitarCambioContrasena;

public interface ISolicitarCambioContrasenaRepositorio
{
    Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken);

    /// <summary>Primer nombre de alguna persona con ese Acceso (para el saludo del correo), o null.</summary>
    Task<string?> BuscarPrimerNombreAsync(Guid accesoId, CancellationToken cancellationToken);
}
