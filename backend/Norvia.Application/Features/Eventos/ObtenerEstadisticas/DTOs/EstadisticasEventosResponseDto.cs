using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ObtenerEstadisticas.DTOs;

/// <summary>Eventos de un tipo en el periodo y cuántos de ellos fueron graves.</summary>
public sealed record EventosPorTipoDto(TipoAccidente Tipo, int Cantidad, int Graves);

/// <summary>Eventos de un mes del periodo (mes de Honduras).</summary>
public sealed record EventosPorMesDto(int Anio, int Mes, int Cantidad, int Graves);

/// <summary>
/// Estadísticas de eventos (HU-052, art. 47.e): solo cifras, sin datos de personas (D11). No cuentan los descartados
/// (RN-28). <paramref name="PorTipo"/> trae los cuatro tipos en orden fijo y <paramref name="PorMes"/> todos los meses del
/// periodo, también con 0, para que se comparen sobre la misma base.
/// </summary>
public sealed record EstadisticasEventosResponseDto(
    DateOnly Desde,
    DateOnly Hasta,
    int Total,
    int Graves,
    IReadOnlyList<EventosPorTipoDto> PorTipo,
    IReadOnlyList<EventosPorMesDto> PorMes);
