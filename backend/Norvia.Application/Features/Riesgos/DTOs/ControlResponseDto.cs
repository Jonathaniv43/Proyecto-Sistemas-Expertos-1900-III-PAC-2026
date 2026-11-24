using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.DTOs;

/// <summary>
/// Control de un riesgo (HU-039): su tipo en la jerarquía (RN-21) y su descripción. <paramref name="PlantillaId"/> es la
/// plantilla del peligro de la que salió, si se aceptó una sugerencia (RN-76); null si es propio.
/// </summary>
public sealed record ControlResponseDto(
    Guid Id,
    Guid RiesgoId,
    TipoControl Tipo,
    string Descripcion,
    EstadoControl Estado,
    Guid? PlantillaId,
    DateTime CreadoEn)
{
    public static ControlResponseDto De(Control c) =>
        new(c.Id, c.RiesgoEmpresaId, c.Tipo, c.Descripcion, c.Estado, c.PlantillaTareaId, c.CreadoEn);
}
