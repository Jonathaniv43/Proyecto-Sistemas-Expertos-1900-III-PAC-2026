using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.CrearControl.DTOs;

/// <summary>
/// Control nuevo de un riesgo (HU-039): tipo (RN-21) y descripción. Al aceptar una sugerencia, con o sin editarla,
/// <see cref="PlantillaId"/> indica la plantilla de la que sale (RN-76).
/// </summary>
public sealed record CrearControlRequestDto
{
    public TipoControl? Tipo { get; init; }
    public string? Descripcion { get; init; }
    public Guid? PlantillaId { get; init; }
}
