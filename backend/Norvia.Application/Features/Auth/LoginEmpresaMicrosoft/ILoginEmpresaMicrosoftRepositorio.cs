using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Auth.LoginEmpresaMicrosoft;

public interface ILoginEmpresaMicrosoftRepositorio
{
    Task<Acceso?> BuscarAccesoPorProveedorAsync(ProveedorIdentidad proveedor, string proveedorUserId, CancellationToken cancellationToken);

    /// <remarks>Incluye <c>Empresa</c> y <c>Rol</c>.</remarks>
    Task<VinculoEmpresa?> BuscarVinculoAsync(Guid accesoId, Guid empresaId, CancellationToken cancellationToken);

    Task<bool> UsuarioActivoAsync(Guid personaId, CancellationToken cancellationToken);

    /// <summary>Deja la fila pendiente; no guarda.</summary>
    void AgregarRefresh(RefreshToken refresh);
}
