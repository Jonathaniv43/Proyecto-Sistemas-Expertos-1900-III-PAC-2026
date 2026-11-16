using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Encuestas.ListarTiposRespuesta.DTOs;

public sealed record TipoRespuestaPreguntaResponseDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string InstruccionUi,
    bool RequiereOpciones,
    short? MaxCaracteresRespuesta)
{
    public static TipoRespuestaPreguntaResponseDto De(TipoRespuestaPreguntaCatalogo t) => new(
        t.Id, t.Codigo, t.Nombre, t.InstruccionUi, t.RequiereOpciones, t.MaxCaracteresRespuesta);
}
