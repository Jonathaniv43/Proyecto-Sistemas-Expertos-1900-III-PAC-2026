using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ObtenerEvento.DTOs;

/// <summary>
/// HU-047: detalle del evento reportado de forma anónima.
/// <para>
/// Pendiente de futuras HUs:
/// - HU-100: agregar colección de evidencias (<c>Archivo</c>).
/// - HU-049: agregar datos de cierre/descarte e <c>InvestigacionAccidente</c>.
/// </para>
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
    DateTime RegistradoEn);
