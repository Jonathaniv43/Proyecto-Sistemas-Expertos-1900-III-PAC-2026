using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Eventos.ObtenerEstadisticas;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Eventos;

// Accidente es tabla de empresa: el filtro global limita a la sesión. Solo se leen tipo, gravedad y fecha.
public sealed class ObtenerEstadisticasRepositorio(NorviaDbContext db) : IObtenerEstadisticasRepositorio
{
    public Task<List<EventoParaContar>> ListarAsync(DateTime desdeUtc, DateTime hastaUtc, CancellationToken ct) =>
        db.Accidente.AsNoTracking()
            .Where(a => a.Estado != EstadoAccidente.Descartado && a.FechaHoraEvento >= desdeUtc && a.FechaHoraEvento < hastaUtc)
            .Select(a => new EventoParaContar(a.Tipo, a.EsGrave, a.FechaHoraEvento))
            .ToListAsync(ct);
}
