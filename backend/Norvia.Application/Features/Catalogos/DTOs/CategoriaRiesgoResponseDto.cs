using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.DTOs;

public sealed record CategoriaRiesgoResponseDto(Guid Id, short RubroId, string Nombre, string? Descripcion, bool Activa)
{
    public static CategoriaRiesgoResponseDto De(CategoriaRiesgo c) => new(c.Id, c.RubroId, c.Nombre, c.Descripcion, c.Activa);
}
