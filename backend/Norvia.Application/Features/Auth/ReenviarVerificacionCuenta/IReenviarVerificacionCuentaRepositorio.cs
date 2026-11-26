using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Auth.ReenviarVerificacionCuenta;

public interface IReenviarVerificacionCuentaRepositorio
{
    Task<Acceso?> BuscarAccesoPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken);
}
