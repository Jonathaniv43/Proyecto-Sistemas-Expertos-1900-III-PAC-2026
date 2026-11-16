using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.DTOs;

public sealed record OpcionPreguntaResponseDto(Guid Id, string Texto, short Orden, bool IndicaExposicion);

public sealed record PreguntaResponseDto(
    Guid Id,
    Guid CategoriaRiesgoId,
    string Texto,
    string TipoRespuestaCodigo,
    string InstruccionUi,
    bool Obligatoria,
    short Orden,
    Guid? PeligroId,
    bool Activa,
    IReadOnlyList<OpcionPreguntaResponseDto> Opciones)
{
    public static PreguntaResponseDto De(PreguntaExploratoria p)
    {
        var tipo = p.TipoRespuestaPregunta;
        var codigo = tipo?.Codigo ?? string.Empty;
        var instruccion = tipo?.InstruccionUi
            ?? TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(codigo);
        return new(
            p.Id, p.CategoriaRiesgoId, p.Texto, codigo, instruccion,
            p.Obligatoria, p.Orden, p.PeligroId, p.Activa,
            p.Opciones.OrderBy(o => o.Orden)
                .Select(o => new OpcionPreguntaResponseDto(o.Id, o.Texto, o.Orden, o.IndicaExposicion))
                .ToList());
    }
}
