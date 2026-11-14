using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.LoginEmpresa;

public interface ILoginEmpresaRepositorio
{
    Task<Acceso?> BuscarAccesoAsync(string emailNormalizado, CancellationToken cancellationToken);

    /// <remarks>Incluye <c>Empresa</c> y <c>Rol</c>.</remarks>
    Task<VinculoEmpresa?> BuscarVinculoAsync(Guid accesoId, Guid empresaId, CancellationToken cancellationToken);

    /// <summary>Deja la fila pendiente; no guarda.</summary>
    void AgregarRefresh(RefreshToken refresh);
}
