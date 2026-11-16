using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.ListarEncuestas.DTOs;

public sealed record EncuestaResumenResponseDto(
    Guid Id,
    string Titulo,
    EstadoEncuesta Estado,
    DateTime CreadaEn,
    int? VersionPublicadaNumero,
    int Participaciones);
