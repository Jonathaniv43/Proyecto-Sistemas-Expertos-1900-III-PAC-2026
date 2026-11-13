using Norvia.Application.Features.Auth.DTOs;

namespace Norvia.Application.Features.Auth.Interfaces;

public interface IAuthServicio
{
    Task<ResultadoAuth> LoginEmpresaAsync(LoginEmpresaRequest request, CancellationToken cancellationToken = default);
    Task<ResultadoAuth> LoginPlataformaAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ResultadoAuth> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}
