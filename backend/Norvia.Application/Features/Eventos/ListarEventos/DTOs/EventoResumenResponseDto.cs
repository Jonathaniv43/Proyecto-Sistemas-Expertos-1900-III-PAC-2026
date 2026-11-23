using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ListarEventos.DTOs;

public sealed record EventoResumenResponseDto(
    Guid Id,
    TipoAccidente Tipo,
    EstadoAccidente Estado,
    EstadoEnfermedad? EstadoEnfermedad,
    bool EsGrave,
    DateTime FechaHoraEvento,
    string? Lugar,
    string Descripcion,
    DateTime RegistradoEn);
