using Norvia.Application.Features.Riesgos.ListarControlesSugeridos.DTOs;

namespace Norvia.Application.Features.Riesgos.ListarControlesSugeridos;

public interface IListarControlesSugeridosRepositorio
{
    /// <summary>Peligro del riesgo de la empresa de la sesión (null si es descrito); <c>Existe</c> es false si el riesgo no existe.</summary>
    Task<(Guid? PeligroId, bool Existe)> PeligroDelRiesgoAsync(Guid riesgoId, CancellationToken ct);

    /// <summary>Plantillas activas del peligro con tipo de control que el riesgo todavía no aceptó.</summary>
    Task<IReadOnlyList<ControlSugeridoResponseDto>> PendientesAsync(Guid riesgoId, Guid peligroId, CancellationToken ct);
}
