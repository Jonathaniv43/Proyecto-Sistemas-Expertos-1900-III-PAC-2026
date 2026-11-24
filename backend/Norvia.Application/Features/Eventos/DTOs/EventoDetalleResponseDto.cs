using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.DTOs;

/// <summary>Investigación de un evento (HU-049).</summary>
public sealed record InvestigacionResponseDto(
    string CausasInmediatas, string CausasBasicas, string? Conclusiones, Guid InvestigadaPorId, string InvestigadaPorNombre,
    DateOnly FechaInvestigacion);

/// <summary>
/// Detalle de un evento reportado de forma anónima (HU-047): no trae quién lo reportó porque no se guarda (RN-27).
/// HU-049: su investigación, cierre o descarte (con quién descartó y por qué). <paramref name="TotalMedidas"/> cuenta
/// las medidas correctivas (tareas con origen en el evento, HU-050) que no están canceladas; para cerrar hace falta al
/// menos una (RN-28). Pendiente HU-100: evidencias.
/// </summary>
public sealed record EventoDetalleResponseDto(
    Guid Id,
    TipoAccidente Tipo,
    EstadoAccidente Estado,
    EstadoEnfermedad? EstadoEnfermedad,
    bool EsGrave,
    DateTime FechaHoraEvento,
    string? Lugar,
    string Descripcion,
    DateTime RegistradoEn,
    InvestigacionResponseDto? Investigacion,
    DateTime? CerradoEn,
    DateTime? DescartadoEn,
    string? DescartadoPorNombre,
    string? MotivoDescarte,
    int TotalMedidas);
