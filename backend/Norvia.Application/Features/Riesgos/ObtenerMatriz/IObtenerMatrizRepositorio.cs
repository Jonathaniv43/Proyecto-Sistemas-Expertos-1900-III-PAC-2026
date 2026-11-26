namespace Norvia.Application.Features.Riesgos.ObtenerMatriz;

public interface IObtenerMatrizRepositorio
{
    /// <summary>
    /// Por cada riesgo confirmado de la empresa de la sesión, la probabilidad y severidad de su última evaluación
    /// inicial (null si no tiene) y si tiene controles.
    /// </summary>
    Task<IReadOnlyList<(short? Probabilidad, short? Severidad, bool TieneControles)>> UbicacionesAsync(CancellationToken ct);
}
