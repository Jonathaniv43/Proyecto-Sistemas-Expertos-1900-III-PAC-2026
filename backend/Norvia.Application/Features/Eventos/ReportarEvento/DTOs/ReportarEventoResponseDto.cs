using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ReportarEvento.DTOs;

public sealed record ReportarEventoResponseDto(
    Guid Id,
    TipoAccidente Tipo,
    EstadoAccidente Estado,
    EstadoEnfermedad? EstadoEnfermedad,
    bool EsGrave,
    DateTime FechaHoraEvento,
    DateTime RegistradoEn);
