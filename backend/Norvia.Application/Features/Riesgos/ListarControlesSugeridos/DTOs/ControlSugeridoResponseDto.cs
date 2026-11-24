using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Riesgos.ListarControlesSugeridos.DTOs;

/// <summary>Control que sugiere una plantilla del peligro del riesgo (RN-76). No se guarda hasta aceptarlo.</summary>
public sealed record ControlSugeridoResponseDto(Guid PlantillaId, TipoControl Tipo, string Titulo, string? Descripcion);
