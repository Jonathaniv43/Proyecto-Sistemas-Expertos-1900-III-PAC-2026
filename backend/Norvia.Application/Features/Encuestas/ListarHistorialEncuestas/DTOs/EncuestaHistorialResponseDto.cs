using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ListarHistorialEncuestas.DTOs;

public sealed record ResumenHistorialEncuestaDto(
    int Participaciones,
    int RespuestasAnonimas);

public sealed record EncuestaHistorialResponseDto(
    Guid Id,
    string Titulo,
    EstadoEncuesta Estado,
    DateTime CreadaEn,
    DateTime? CerradaEn,
    DateTime? UltimaPublicacionEn,
    IReadOnlyList<string> Categorias,
    ResumenHistorialEncuestaDto Resumen,
    IReadOnlyList<VersionEncuestaResumenResponseDto> Versiones);
