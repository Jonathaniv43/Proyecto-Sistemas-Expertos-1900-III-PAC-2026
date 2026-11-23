using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ObtenerPeligrosSugeridos.DTOs;

/// <summary>Pregunta que originó la sugerencia y cuántas respuestas eligieron una opción de exposición en ella.</summary>
public sealed record PreguntaOrigenDto(Guid PreguntaId, string Texto, int RespuestasConExposicion);

/// <summary>
/// Peligro sugerido por la encuesta (RN-13). <paramref name="RiesgoSugeridoId"/> es el <c>RiesgoEmpresa</c>
/// que se confirma o descarta en HU-035; <paramref name="Estado"/> indica si ya se revisó.
/// </summary>
public sealed record PeligroSugeridoDto(
    Guid RiesgoSugeridoId,
    Guid PeligroId,
    string PeligroNombre,
    EstadoRiesgo Estado,
    int RespuestasConExposicion,
    IReadOnlyList<PreguntaOrigenDto> Preguntas);

/// <param name="DetalleOculto">Siempre false: el detalle se muestra con cualquier cantidad de respuestas.</param>
public sealed record PeligrosSugeridosResponseDto(
    Guid EncuestaId,
    int TotalRespuestas,
    int MinimoRespuestas,
    bool DetalleOculto,
    IReadOnlyList<PeligroSugeridoDto> Sugerencias);
