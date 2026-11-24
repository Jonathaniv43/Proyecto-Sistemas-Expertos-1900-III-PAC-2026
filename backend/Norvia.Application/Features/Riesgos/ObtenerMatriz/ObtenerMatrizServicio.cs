using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Features.Riesgos.ObtenerMatriz.DTOs;
using Norvia.Domain.Entities.Riesgos;

namespace Norvia.Application.Features.Riesgos.ObtenerMatriz;

/// <summary>HU-038: cuántos riesgos de la matriz hay en cada celda de probabilidad × severidad (art. 47.b).</summary>
public sealed class ObtenerMatrizServicio(IObtenerMatrizRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<MatrizRiesgosResponseDto> EjecutarAsync(CancellationToken ct)
    {
        var ubicaciones = await repositorio.UbicacionesAsync(ct);
        var celdas = ubicaciones
            .Where(u => u.Probabilidad is not null && u.Severidad is not null)
            .GroupBy(u => (P: u.Probabilidad!.Value, S: u.Severidad!.Value))
            .Select(g => new CeldaMatrizDto(g.Key.P, g.Key.S, g.Key.P * g.Key.S, EvaluacionRiesgo.Clasificar(g.Key.P * g.Key.S), g.Count()))
            .OrderByDescending(c => c.Probabilidad).ThenBy(c => c.Severidad)
            .ToList();
        return new MatrizRiesgosResponseDto(ubicaciones.Count, ubicaciones.Count(u => u.Probabilidad is null), celdas);
    }
}
