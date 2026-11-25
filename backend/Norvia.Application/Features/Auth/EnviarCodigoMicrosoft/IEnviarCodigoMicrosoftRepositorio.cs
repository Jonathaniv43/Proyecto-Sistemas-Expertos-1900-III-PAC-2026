using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.EnviarCodigoMicrosoft;

public interface IEnviarCodigoMicrosoftRepositorio
{
    Task<bool> ProveedorVinculadoAsync(ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken);
    Task<bool> ExisteEmailAsync(string emailNormalizado, CancellationToken cancellationToken);

    /// <summary>La fila de esa persona y correo (RN-86), rastreada para poder modificarla, o null.</summary>
    Task<VerificacionCorreoMicrosoft?> BuscarAsync(string email, string proveedorUserId, CancellationToken cancellationToken);

    /// <summary>Deja la fila pendiente; no guarda.</summary>
    void Agregar(VerificacionCorreoMicrosoft fila);
}
