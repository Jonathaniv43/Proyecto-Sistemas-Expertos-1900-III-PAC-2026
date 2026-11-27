using Norvia.Application.Features.Encuestas.ObtenerReporteEncuesta.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.CompararEncuestas.DTOs;

public sealed record LadoComparacionDto(
    Guid EncuestaId,
    string Titulo,
    EstadoEncuesta EstadoEncuesta,
    Guid VersionId,
    int NumeroVersion,
    EstadoVersionEncuesta EstadoVersion,
    DateTime? PublicadaEn,
    DateTime? CerradaEn,
    int Participaciones,
    int TotalRespuestasAnonimas,
    DateOnly? PeriodoDesde,
    DateOnly? PeriodoHasta,
    bool DetalleOculto,
    IReadOnlyList<string> Categorias,
    int CantidadRespuestasBreves);

public sealed record OpcionComparadaDto(
    string Etiqueta,
    int CantidadBase,
    double PorcentajeBase,
    int CantidadComparar,
    double PorcentajeComparar,
    double DiferenciaPuntosPorcentuales);

public sealed record PreguntaComparadaDto(
    string ClaveEmparejamiento,
    string TextoBase,
    string TextoComparar,
    string TipoRespuesta,
    int RespondieronBase,
    int RespondieronComparar,
    IReadOnlyList<OpcionComparadaDto> Opciones);

public sealed record PeligroComparadoDto(
    Guid PeligroId,
    string PeligroNombre,
    int RespuestasBase,
    double PorcentajeBase,
    EstadoRiesgo? EstadoEnEncuestaBase,
    int RespuestasComparar,
    double PorcentajeComparar,
    EstadoRiesgo? EstadoEnEncuestaComparar,
    double DiferenciaPuntosPorcentuales);

public sealed record ComparacionEncuestasResponseDto(
    LadoComparacionDto Base,
    LadoComparacionDto Comparar,
    int MinimoRespuestas,
    bool ComparacionOculta,
    IReadOnlyList<PreguntaComparadaDto> PreguntasComparadas,
    IReadOnlyList<PreguntaReporteResponseDto> PreguntasSoloBase,
    IReadOnlyList<PreguntaReporteResponseDto> PreguntasSoloComparar,
    IReadOnlyList<PeligroComparadoDto> PeligrosComparados);
