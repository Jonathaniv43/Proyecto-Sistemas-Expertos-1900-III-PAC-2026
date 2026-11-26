using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.DTOs;

public sealed record PeligroResponseDto(
    Guid Id, string Codigo, string Nombre, string? Descripcion, ClasificacionPeligro Clasificacion, bool Activo)
{
    public static PeligroResponseDto De(Peligro p) => new(p.Id, p.Codigo, p.Nombre, p.Descripcion, p.Clasificacion, p.Activo);
}
