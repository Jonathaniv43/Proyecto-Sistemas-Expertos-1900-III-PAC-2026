using Norvia.Domain.Enums;

namespace Norvia.Application.Features.Eventos.ObtenerEstadisticas;

/// <summary>Lo mínimo de un evento para contarlo: nada que identifique a nadie.</summary>
public sealed record EventoParaContar(TipoAccidente Tipo, bool EsGrave, DateTime FechaHoraEvento);

public interface IObtenerEstadisticasRepositorio
{
    /// <summary>Eventos de la empresa de la sesión que no están descartados y ocurrieron en [desdeUtc, hastaUtc).</summary>
    Task<List<EventoParaContar>> ListarAsync(DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct);
}
