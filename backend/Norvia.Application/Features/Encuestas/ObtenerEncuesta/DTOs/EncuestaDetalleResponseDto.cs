using Norvia.Application.Features.Encuestas.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ObtenerEncuesta.DTOs;

public sealed record EncuestaDetalleResponseDto(
    Guid Id,
    string Titulo,
    EstadoEncuesta Estado,
    Guid CreadaPorId,
    DateTime CreadaEn,
    DateTime? CerradaEn,
    EncuestaVersionResponseDto? VersionPublicada,
    EncuestaVersionResponseDto? VersionBorrador,
    IReadOnlyList<VersionEncuestaResumenResponseDto> VersionesPublicadas);
