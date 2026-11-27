using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.DTOs;

/// <summary>Resumen de una versión publicada o reemplazada de una encuesta (HU-030, HU-034).</summary>
public sealed record VersionEncuestaResumenResponseDto(
    Guid Id,
    int NumeroVersion,
    EstadoVersionEncuesta Estado,
    DateTime? PublicadaEn,
    int TotalRespuestasAnonimas,
    IReadOnlyList<string> Categorias);
