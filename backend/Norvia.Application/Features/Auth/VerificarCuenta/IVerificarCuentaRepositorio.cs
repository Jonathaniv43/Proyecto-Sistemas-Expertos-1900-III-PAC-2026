using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.VerificarCuenta;

public interface IVerificarCuentaRepositorio
{
    Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken);
}
