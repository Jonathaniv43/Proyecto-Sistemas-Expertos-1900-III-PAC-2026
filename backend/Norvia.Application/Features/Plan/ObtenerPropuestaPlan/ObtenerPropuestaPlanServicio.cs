using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Plan.ObtenerPropuestaPlan.DTOs;
using Norvia.Application.Features.Plan.Propuesta;

namespace Norvia.Application.Features.Plan.ObtenerPropuestaPlan;

/// <summary>HU-042: el plan propuesto para revisar antes de confirmarlo. No guarda nada.</summary>
public sealed class ObtenerPropuestaPlanServicio(IObtenerPropuestaPlanRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<PropuestaPlanResponseDto> EjecutarAsync(CancellationToken ct)
    {
        var datos = await repositorio.DatosAsync(ct);
        return new PropuestaPlanResponseDto(PropuestaPlan.Calcular(datos.Riesgos), datos.RiesgosSinEvaluar);
    }
}
