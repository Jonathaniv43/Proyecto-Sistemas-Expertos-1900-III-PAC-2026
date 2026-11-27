using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.VerificarCuenta;

public interface IVerificarCuentaRepositorio
{
    Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken);

    /// <summary>La fila de activación de ese Acceso, rastreada para poder modificarla o eliminarla, o null.</summary>
    Task<VerificacionCorreo?> BuscarVerificacionAsync(Guid accesoId, CancellationToken cancellationToken);

    /// <summary>Deja el borrado pendiente; no guarda.</summary>
    void EliminarVerificacion(VerificacionCorreo fila);
}
