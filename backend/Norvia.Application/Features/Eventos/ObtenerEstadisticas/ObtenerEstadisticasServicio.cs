using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Eventos.ObtenerEstadisticas.DTOs;
using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ObtenerEstadisticas;

/// <summary>
/// HU-052: cuántos eventos hubo en un periodo, por tipo, gravedad y mes, para detectar patrones (art. 47.e).
/// <list type="bullet">
/// <item>El periodo son fechas de Honduras (UTC−6, sin horario de verano), incluidas, sobre cuándo ocurrió el evento.</item>
/// <item>Los descartados no cuentan (RN-28).</item>
/// <item>Solo cifras: ni quién reportó (no se guarda, D11) ni descripciones o lugares.</item>
/// </list>
/// </summary>
public sealed class ObtenerEstadisticasServicio(IObtenerEstadisticasRepositorio repositorio) : IServicioCasoDeUso
{
    private static readonly TimeSpan DiferenciaHonduras = TimeSpan.FromHours(6);

    public async Task<Resultado<EstadisticasEventosResponseDto>> EjecutarAsync(FiltrosEstadisticasDto filtros, CancellationToken ct)
    {
        var desde = filtros.Desde!.Value;
        var hasta = filtros.Hasta!.Value;
        var desdeUtc = desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) + DiferenciaHonduras;
        var hastaUtc = hasta.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) + DiferenciaHonduras;

        var eventos = await repositorio.ListarAsync(desdeUtc, hastaUtc, ct);
        var enHonduras = eventos.Select(e => (e.Tipo, e.EsGrave, Fecha: e.FechaHoraEvento - DiferenciaHonduras)).ToList();

        var porTipo = Enum.GetValues<TipoAccidente>()
            .Select(t => new EventosPorTipoDto(t, enHonduras.Count(e => e.Tipo == t), enHonduras.Count(e => e.Tipo == t && e.EsGrave)))
            .ToList();

        var porMes = new List<EventosPorMesDto>();
        for (var mes = new DateOnly(desde.Year, desde.Month, 1); mes <= hasta; mes = mes.AddMonths(1))
        {
            var delMes = enHonduras.Where(e => e.Fecha.Year == mes.Year && e.Fecha.Month == mes.Month).ToList();
            porMes.Add(new EventosPorMesDto(mes.Year, mes.Month, delMes.Count, delMes.Count(e => e.EsGrave)));
        }

        return new EstadisticasEventosResponseDto(desde, hasta, enHonduras.Count, enHonduras.Count(e => e.EsGrave), porTipo, porMes);
    }
}
