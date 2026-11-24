using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Encuestas.RevisarPeligroSugerido.DTOs;

public sealed record PeligroSugeridoRevisadoResponseDto(
    Guid RiesgoId,
    Guid EncuestaId,
    Guid PeligroId,
    EstadoRiesgo Estado,
    string? Actividad,
    Guid RevisadoPorId,
    DateTime RevisadoEn)
{
    public static PeligroSugeridoRevisadoResponseDto De(RiesgoEmpresa r) =>
        new(r.Id, r.EncuestaId!.Value, r.PeligroId!.Value, r.Estado, r.Actividad, r.RevisadoPorId!.Value, r.RevisadoEn!.Value);
}
