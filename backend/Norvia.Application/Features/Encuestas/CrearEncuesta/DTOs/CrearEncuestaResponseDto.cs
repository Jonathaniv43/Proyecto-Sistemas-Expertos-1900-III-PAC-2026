using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.CrearEncuesta.DTOs;

public sealed record CrearEncuestaResponseDto(
    Guid Id,
    string Titulo,
    EstadoEncuesta Estado,
    Guid VersionBorradorId,
    Guid? VersionPublicadaId);
