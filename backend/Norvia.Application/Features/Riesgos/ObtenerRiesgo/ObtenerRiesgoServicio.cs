using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Riesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ObtenerRiesgo;

/// <summary>Un riesgo de la empresa (HU-036). De otra empresa responde igual que si no existiera (404).</summary>
public sealed class ObtenerRiesgoServicio(IObtenerRiesgoRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<RiesgoResponseDto>> EjecutarAsync(Guid id, CancellationToken ct) =>
        await repositorio.BuscarAsync(id, ct) is { } riesgo ? riesgo : Errores.RiesgoNoEncontrado;
}
