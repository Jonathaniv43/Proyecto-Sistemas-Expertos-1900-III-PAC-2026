using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.VerificarCodigoMicrosoft;

public interface IVerificarCodigoMicrosoftRepositorio
{
    /// <summary>Rastreada para poder modificarla (intentos) o eliminarla (código consumido), o null.</summary>
    Task<VerificacionCorreoMicrosoft?> BuscarAsync(string email, string proveedorUserId, CancellationToken cancellationToken);

    /// <summary>Deja el borrado pendiente; no guarda.</summary>
    void Eliminar(VerificacionCorreoMicrosoft fila);
}
