using Norvia.Application.Features.Riesgos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarControles;

public interface IListarControlesRepositorio
{
    Task<bool> ExisteRiesgoAsync(Guid riesgoId, CancellationToken ct);

    /// <summary>Controles del riesgo, del más efectivo al menos efectivo (RN-21) y, dentro de cada tipo, por antigüedad.</summary>
    Task<IReadOnlyList<ControlResponseDto>> ListarAsync(Guid riesgoId, CancellationToken ct);
}
