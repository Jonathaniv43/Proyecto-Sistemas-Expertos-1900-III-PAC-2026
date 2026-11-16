using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.DTOs;

public sealed record EncuestaSeccionResponseDto(Guid Id, string Titulo, short Orden);

public sealed record EncuestaVersionResponseDto(
    Guid Id,
    int NumeroVersion,
    EstadoVersionEncuesta Estado,
    string? Instrucciones,
    DateTime? PublicadaEn,
    IReadOnlyList<EncuestaSeccionResponseDto> Secciones,
    IReadOnlyList<EncuestaPreguntaResponseDto> Preguntas)
{
    public static EncuestaVersionResponseDto De(EncuestaVersion version) => new(
        version.Id,
        version.NumeroVersion,
        version.Estado,
        version.Instrucciones,
        version.PublicadaEn,
        version.Secciones
            .OrderBy(s => s.Orden)
            .Select(s => new EncuestaSeccionResponseDto(s.Id, s.Titulo, s.Orden))
            .ToList(),
        version.Preguntas
            .OrderBy(p => p.Orden)
            .Select(p =>
            {
                var tipo = p.TipoRespuestaPregunta;
                var codigo = tipo?.Codigo ?? string.Empty;
                var instruccion = tipo?.InstruccionUi
                    ?? TipoRespuestaPreguntaCatalogo.InstruccionUiPorDefecto(codigo);
                return new EncuestaPreguntaResponseDto(
                    p.Id,
                    p.EncuestaSeccionId,
                    p.CategoriaRiesgoId,
                    p.PreguntaExploratoriaId,
                    p.Texto,
                    codigo,
                    instruccion,
                    p.Obligatoria,
                    p.PeligroId,
                    p.Orden,
                    p.Opciones
                        .OrderBy(o => o.Orden)
                        .Select(o => new EncuestaOpcionResponseDto(o.Id, o.Texto, o.Orden, o.IndicaExposicion))
                        .ToList());
            })
            .ToList());
}

public sealed record EncuestaPreguntaResponseDto(
    Guid Id,
    Guid? SeccionId,
    Guid? CategoriaRiesgoId,
    Guid? PreguntaExploratoriaId,
    string Texto,
    string TipoRespuestaCodigo,
    string InstruccionUi,
    bool Obligatoria,
    Guid? PeligroId,
    short Orden,
    IReadOnlyList<EncuestaOpcionResponseDto> Opciones);

public sealed record EncuestaOpcionResponseDto(
    Guid Id,
    string Texto,
    short Orden,
    bool IndicaExposicion);
