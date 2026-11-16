using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Vinculos.ObtenerVinculo;

public interface IObtenerVinculoRepositorio
{
    /// <summary>Solo encuentra vínculos de la empresa de la sesión.</summary>
    Task<VinculoEmpresa?> BuscarAsync(Guid id, CancellationToken ct);
}
