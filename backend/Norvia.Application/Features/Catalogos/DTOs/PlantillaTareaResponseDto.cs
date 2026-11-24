using Norvia.Domain.Entities.Catalogos;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Catalogos.DTOs;

/// <summary>
/// Plantilla de tarea de un peligro (HU-023). Con <paramref name="TipoControl"/>, Norvia la sugiere como control del
/// riesgo (HU-039, RN-76); sin tipo es una tarea modelo que entra directo al plan (HU-042).
/// </summary>
public sealed record PlantillaTareaResponseDto(
    Guid Id, Guid? PeligroId, TipoControl? TipoControl, string Titulo, string? Descripcion, short PrioridadSugerida,
    bool RequiereEvidencia, bool Activa)
{
    public static PlantillaTareaResponseDto De(PlantillaTarea p) =>
        new(p.Id, p.PeligroId, p.TipoControl, p.Titulo, p.Descripcion, p.PrioridadSugerida, p.RequiereEvidencia, p.Activa);
}
