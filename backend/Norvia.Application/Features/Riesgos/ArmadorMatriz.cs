using Norvia.Application.Features.Riesgos.ObtenerMatriz.DTOs;
using Norvia.Domain.Entities.Riesgos;

namespace Norvia.Application.Features.Riesgos;

/// <summary>
/// Calcula el resumen y las celdas del mapa de calor 5 × 5 a partir de la última evaluación inicial
/// y la presencia de controles de los riesgos confirmados (HU-038, HU-075, RN-19).
/// </summary>
public static class ArmadorMatriz
{
    public static MatrizRiesgosResponseDto Construir(
        IReadOnlyCollection<(short? Probabilidad, short? Severidad, bool TieneControles)> ubicaciones)
    {
        var celdas = ubicaciones
            .Where(u => u.Probabilidad is not null && u.Severidad is not null)
            .GroupBy(u => (P: u.Probabilidad!.Value, S: u.Severidad!.Value))
            .Select(g => new CeldaMatrizDto(g.Key.P, g.Key.S, g.Key.P * g.Key.S, EvaluacionRiesgo.Clasificar(g.Key.P * g.Key.S), g.Count()))
            .OrderByDescending(c => c.Probabilidad)
            .ThenBy(c => c.Severidad)
            .ToList();

        // RN-19: los altos y críticos sin controles se señalan.
        var altosSinControl = ubicaciones.Count(u => u.Probabilidad * u.Severidad >= 10 && !u.TieneControles);
        return new MatrizRiesgosResponseDto(
            ubicaciones.Count,
            ubicaciones.Count(u => u.Probabilidad is null),
            altosSinControl,
            celdas);
    }
}
