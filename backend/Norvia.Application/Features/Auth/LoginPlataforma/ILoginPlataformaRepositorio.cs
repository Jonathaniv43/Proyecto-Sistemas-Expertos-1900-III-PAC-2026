using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.LoginPlataforma;

public interface ILoginPlataformaRepositorio
{
    Task<Acceso?> BuscarAccesoAsync(string emailNormalizado, CancellationToken cancellationToken);

    /// <remarks>Incluye <c>RolStaffPlataforma</c>.</remarks>
    Task<StaffPlataforma?> BuscarStaffAsync(Guid accesoId, CancellationToken cancellationToken);

    /// <summary>Deja la fila pendiente; no guarda.</summary>
    void AgregarRefresh(RefreshToken refresh);
}
