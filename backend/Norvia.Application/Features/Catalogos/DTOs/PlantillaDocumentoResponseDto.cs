using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.DTOs;

public sealed record PlantillaDocumentoResponseDto(
    Guid Id, string Codigo, string Titulo, TipoDocumento TipoDocumento, short? RubroId, string Contenido, bool Activa)
{
    public static PlantillaDocumentoResponseDto De(PlantillaDocumento p) => new(
        p.Id, p.Codigo, p.Titulo, p.TipoDocumento, p.RubroId, p.Contenido, p.Activa);
}
