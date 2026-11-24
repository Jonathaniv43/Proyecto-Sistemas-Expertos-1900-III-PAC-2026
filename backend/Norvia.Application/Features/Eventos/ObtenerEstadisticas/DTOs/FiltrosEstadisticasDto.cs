namespace Norvia.Application.Features.Eventos.ObtenerEstadisticas.DTOs;

/// <summary>Periodo de las estadísticas (HU-052): fechas de Honduras, incluidas, sobre cuándo ocurrió el evento.</summary>
public sealed record FiltrosEstadisticasDto
{
    public DateOnly? Desde { get; init; }
    public DateOnly? Hasta { get; init; }
}
