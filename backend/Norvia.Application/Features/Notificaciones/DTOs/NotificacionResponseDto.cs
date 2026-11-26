using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Notificaciones.DTOs;

/// <summary>
/// Una notificación de la bandeja (HU-117). El front arma el enlace al elemento relacionado con <see cref="Tipo"/> y el
/// id de origen: <see cref="TareaId"/> (tarea asignada) o <see cref="VersionDocumentoId"/> (política publicada). Para la
/// política, <see cref="DocumentoId"/> abre su versión vigente (HU-074, HU-116).
/// </summary>
public sealed record NotificacionResponseDto(
    Guid Id,
    TipoNotificacion Tipo,
    string Titulo,
    string Mensaje,
    DateTime CreadaEn,
    DateTime? LeidaEn,
    Guid? TareaId,
    Guid? VersionDocumentoId,
    Guid? DocumentoId);

/// <summary>HU-117: las notificaciones de la persona, de la más reciente a la más antigua, y cuántas no ha leído.</summary>
public sealed record MisNotificacionesResponseDto(int NoLeidas, IReadOnlyList<NotificacionResponseDto> Notificaciones);

/// <summary>HU-117: contador de no leídas (la campana del encabezado).</summary>
public sealed record NoLeidasResponseDto(int NoLeidas);
