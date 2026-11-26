using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Features.Riesgos.ObtenerReporteMatriz.DTOs;

namespace Norvia.Application.Features.Riesgos.ObtenerReporteMatriz;

/// <summary>
/// HU-075: consolida los datos empresariales, el mapa de calor 5 × 5 y los riesgos confirmados con
/// evaluación inicial, residual y controles al solicitarse (RN-52, art. 47.b). No persiste archivos.
/// </summary>
public sealed class ObtenerReporteMatrizServicio(
    IObtenerReporteMatrizRepositorio repositorio,
    IReloj reloj) : IServicioCasoDeUso
{
    public async Task<ReporteMatrizResponseDto> EjecutarAsync(CancellationToken ct)
    {
        var (empresa, riesgos) = await repositorio.ObtenerDatosAsync(ct);
        var generadoEn = await reloj.AhoraAsync(ct);

        var ubicaciones = riesgos
            .Select(r => ((short?)r.Evaluacion?.Probabilidad, (short?)r.Evaluacion?.Severidad, r.Controles.Count > 0))
            .ToList();
        var matriz = ArmadorMatriz.Construir(ubicaciones);

        return new ReporteMatrizResponseDto(generadoEn, empresa, matriz, riesgos);
    }
}
