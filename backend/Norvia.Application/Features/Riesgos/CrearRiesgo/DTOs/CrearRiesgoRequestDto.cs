namespace Norvia.Application.Features.Riesgos.CrearRiesgo.DTOs;

/// <summary>Riesgo agregado a mano (HU-036): un peligro del catálogo o uno descrito, y la actividad relacionada.</summary>
public sealed record CrearRiesgoRequestDto
{
    /// <summary>Peligro del catálogo. Se envía este o <see cref="DescripcionPeligro"/>, no los dos.</summary>
    public Guid? PeligroId { get; init; }

    /// <summary>Peligro que no está en el catálogo, descrito por quien lo agrega.</summary>
    public string? DescripcionPeligro { get; init; }

    /// <summary>Actividad o lugar donde se presenta el riesgo.</summary>
    public string Actividad { get; init; } = string.Empty;

    /// <summary>HU-051: el evento que reveló el riesgo, si se agrega a raíz de uno.</summary>
    public Guid? AccidenteId { get; init; }
}
