using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.CrearControl;

public interface ICrearControlRepositorio
{
    /// <summary>Estado y peligro del riesgo de la empresa de la sesión, o null si no existe.</summary>
    Task<(EstadoRiesgo Estado, Guid? PeligroId)?> RiesgoAsync(Guid riesgoId, CancellationToken ct);

    /// <summary>true si la plantilla está activa, es del peligro y sugiere un tipo de control.</summary>
    Task<bool> EsSugerenciaDelPeligroAsync(Guid plantillaId, Guid peligroId, CancellationToken ct);

    /// <summary>true si el riesgo ya tiene un control que salió de esa plantilla.</summary>
    Task<bool> SugerenciaYaAceptadaAsync(Guid riesgoId, Guid plantillaId, CancellationToken ct);

    void Agregar(Control control);
}
