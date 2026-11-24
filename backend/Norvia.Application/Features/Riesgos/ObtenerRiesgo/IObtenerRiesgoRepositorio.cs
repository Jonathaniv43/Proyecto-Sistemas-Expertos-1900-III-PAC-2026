using Norvia.Application.Features.Riesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ObtenerRiesgo;

public interface IObtenerRiesgoRepositorio
{
    /// <summary>Riesgo de la empresa de la sesión (en cualquier estado), o null.</summary>
    Task<RiesgoResponseDto?> BuscarAsync(Guid id, CancellationToken ct);
}
