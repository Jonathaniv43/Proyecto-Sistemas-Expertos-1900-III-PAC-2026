using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Vinculos.ListarVinculos;

public interface IListarVinculosRepositorio
{
    /// <summary>Vínculos de la empresa de la sesión, con persona, acceso y rol.</summary>
    Task<List<VinculoEmpresa>> ListarAsync(EstadoVinculo? estado, string? rolCodigo, CancellationToken ct);
}
