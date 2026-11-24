using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Riesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarRiesgos;

/// <summary>
/// Riesgos de la matriz de la empresa (HU-036): los confirmados, vengan de una encuesta (HU-035) o agregados a mano.
/// Los sugeridos sin revisar y los descartados no forman parte de la matriz (RN-18).
/// </summary>
public sealed class ListarRiesgosServicio(IListarRiesgosRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<IReadOnlyList<RiesgoResponseDto>> EjecutarAsync(CancellationToken ct) => repositorio.ListarConfirmadosAsync(ct);
}
