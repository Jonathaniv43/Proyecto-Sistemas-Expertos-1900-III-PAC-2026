using Norvia.Application.Features.Riesgos.ObtenerReporteMatriz.DTOs;

namespace Norvia.Application.Features.Riesgos.ObtenerReporteMatriz;

public interface IObtenerReporteMatrizRepositorio
{
    /// <summary>
    /// Incluye Empresa (con Rubro), riesgos confirmados de la empresa de la sesión con Peligro, Encuesta origen,
    /// evaluaciones inicial y residual (con EvaluadoPor y Persona) y todos sus Controles.
    /// </summary>
    Task<(EmpresaReporteMatrizDto Empresa, IReadOnlyList<RiesgoReporteMatrizDto> Riesgos)> ObtenerDatosAsync(CancellationToken ct);
}
