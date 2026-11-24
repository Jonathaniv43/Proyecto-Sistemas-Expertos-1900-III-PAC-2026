using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ListarRiesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarRiesgos;

public interface IListarRiesgosRepositorio
{
    /// <summary>
    /// Riesgos de la empresa de la sesión con los filtros aplicados: primero los de nivel más alto
    /// (los sin evaluar al final) y, a igual nivel, los más recientes.
    /// </summary>
    Task<IReadOnlyList<RiesgoResponseDto>> ListarAsync(FiltrosRiesgosDto filtros, CancellationToken ct);
}
