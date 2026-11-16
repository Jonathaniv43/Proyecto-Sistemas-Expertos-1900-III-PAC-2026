using Norvia.Domain.Entities.Encuestas;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.DTOs;

public sealed record EncuestaVersionResponseDto(
    Guid Id,
    int NumeroVersion,
    EstadoVersionEncuesta Estado,
    string? Instrucciones,
    DateTime? PublicadaEn,
    IReadOnlyList<EncuestaPreguntaResponseDto> Preguntas)
{
    public static EncuestaVersionResponseDto De(EncuestaVersion version) => new(
        version.Id,
        version.NumeroVersion,
        version.Estado,
        version.Instrucciones,
        version.PublicadaEn,
        version.Preguntas
            .OrderBy(p => p.Orden)
            .Select(p => new EncuestaPreguntaResponseDto(
                p.Id,
                p.PreguntaExploratoriaId,
                p.Texto,
                p.TipoRespuesta,
                p.Obligatoria,
                p.PeligroId,
                p.Orden,
                p.Opciones
                    .OrderBy(o => o.Orden)
                    .Select(o => new EncuestaOpcionResponseDto(o.Id, o.Texto, o.Orden, o.IndicaExposicion))
                    .ToList()))
            .ToList());
}

public sealed record EncuestaPreguntaResponseDto(
    Guid Id,
    Guid? PreguntaExploratoriaId,
    string Texto,
    TipoRespuestaPregunta TipoRespuesta,
    bool Obligatoria,
    Guid? PeligroId,
    short Orden,
    IReadOnlyList<EncuestaOpcionResponseDto> Opciones);

public sealed record EncuestaOpcionResponseDto(
    Guid Id,
    string Texto,
    short Orden,
    bool IndicaExposicion);
