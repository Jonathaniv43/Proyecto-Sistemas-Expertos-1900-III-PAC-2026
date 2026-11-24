using Norvia.Application.Features.Riesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarRiesgos;

public interface IListarRiesgosRepositorio
{
    /// <summary>Riesgos confirmados de la empresa de la sesión, los más recientes primero.</summary>
    Task<IReadOnlyList<RiesgoResponseDto>> ListarConfirmadosAsync(CancellationToken ct);
}
