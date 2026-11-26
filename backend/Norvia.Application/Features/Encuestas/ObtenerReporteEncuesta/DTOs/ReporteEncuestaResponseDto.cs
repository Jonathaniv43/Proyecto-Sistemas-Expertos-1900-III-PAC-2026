using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta.DTOs;

public sealed record DistribucionOpcionResponseDto(
    Guid OpcionId,
    string Etiqueta,
    int Cantidad,
    double Porcentaje);

public sealed record PreguntaReporteResponseDto(
    Guid PreguntaId,
    string Texto,
    string TipoRespuesta,
    IReadOnlyList<DistribucionOpcionResponseDto>? Distribucion,
    IReadOnlyList<string>? TextosBreves);

public sealed record ReporteEncuestaResponseDto(
    Guid EncuestaId,
    int TotalParticipaciones,
    int TotalRespuestasAnonimas,
    DateOnly? PeriodoDesde,
    DateOnly? PeriodoHasta,
    bool DetalleOculto,
    IReadOnlyList<PreguntaReporteResponseDto> Preguntas,
    Guid VersionId,
    int NumeroVersion,
    EstadoVersionEncuesta EstadoVersion);
