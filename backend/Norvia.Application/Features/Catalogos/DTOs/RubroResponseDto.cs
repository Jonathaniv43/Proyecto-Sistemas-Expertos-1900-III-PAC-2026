using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.DTOs;

public sealed record RubroResponseDto(short Id, string Codigo, string Nombre, bool Activo)
{
    public static RubroResponseDto De(Rubro r) => new(r.Id, r.Codigo, r.Nombre, r.Activo);
}
