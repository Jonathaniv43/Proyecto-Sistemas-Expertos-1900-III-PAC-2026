using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.ReenviarVerificacionCuenta;

public interface IReenviarVerificacionCuentaRepositorio
{
    Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken);

    /// <summary>La fila de activación de ese Acceso, rastreada para poder modificarla, o null.</summary>
    Task<VerificacionCorreo?> BuscarVerificacionAsync(Guid accesoId, CancellationToken cancellationToken);

    /// <summary>Deja la fila pendiente; no guarda.</summary>
    void AgregarVerificacion(VerificacionCorreo fila);
}
