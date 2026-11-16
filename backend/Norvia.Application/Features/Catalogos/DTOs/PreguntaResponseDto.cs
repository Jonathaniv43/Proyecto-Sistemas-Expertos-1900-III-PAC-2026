using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.DTOs;

public sealed record OpcionPreguntaResponseDto(Guid Id, string Texto, short Orden, bool IndicaExposicion);

public sealed record PreguntaResponseDto(
    Guid Id,
    Guid CategoriaRiesgoId,
    string Texto,
    TipoRespuestaPregunta TipoRespuesta,
    bool Obligatoria,
    short Orden,
    Guid? PeligroId,
    bool Activa,
    IReadOnlyList<OpcionPreguntaResponseDto> Opciones)
{
    public static PreguntaResponseDto De(PreguntaExploratoria p) => new(
        p.Id, p.CategoriaRiesgoId, p.Texto, p.TipoRespuesta, p.Obligatoria, p.Orden, p.PeligroId, p.Activa,
        p.Opciones.OrderBy(o => o.Orden)
            .Select(o => new OpcionPreguntaResponseDto(o.Id, o.Texto, o.Orden, o.IndicaExposicion))
            .ToList());
}
