using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Riesgos.DTOs;
using Norvia.Application.Features.Riesgos.ListarRiesgos.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.ListarRiesgos;

/// <summary>
/// Lista de riesgos de la empresa (HU-036, HU-038), filtrable por peligro, su clasificación, nivel, celda y estado.
/// Sin estado se listan los de la matriz (confirmados); los sugeridos y descartados se piden aparte (RN-18).
/// El orden ayuda a decidir qué atender primero: nivel más alto primero.
/// </summary>
public sealed class ListarRiesgosServicio(IListarRiesgosRepositorio repositorio) : IServicioCasoDeUso
{
    public Task<IReadOnlyList<RiesgoResponseDto>> EjecutarAsync(FiltrosRiesgosDto filtros, CancellationToken ct) =>
        repositorio.ListarAsync(filtros with
        {
            Estado = filtros.Estado ?? EstadoRiesgo.Confirmado,
            Buscar = string.IsNullOrWhiteSpace(filtros.Buscar) ? null : filtros.Buscar.Trim()
        }, ct);
}
