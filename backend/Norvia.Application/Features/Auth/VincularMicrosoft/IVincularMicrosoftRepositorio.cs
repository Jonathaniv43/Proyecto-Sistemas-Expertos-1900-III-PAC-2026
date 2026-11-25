using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.VincularMicrosoft;

public interface IVincularMicrosoftRepositorio
{
    Task<bool> ProveedorVinculadoAsync(ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken);
    Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken);

    /// <summary>Deja la fila pendiente; no guarda.</summary>
    void Agregar(AccesoProveedor proveedor);
}
