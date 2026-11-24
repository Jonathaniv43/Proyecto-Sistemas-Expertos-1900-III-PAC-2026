using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.ObtenerMatriz;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa y EvaluacionRiesgo son tablas de empresa: el filtro global limita a la empresa de la sesión.
public sealed class ObtenerMatrizRepositorio(NorviaDbContext db) : IObtenerMatrizRepositorio
{
    public async Task<IReadOnlyList<(short? Probabilidad, short? Severidad)>> UbicacionesAsync(CancellationToken ct)
    {
        var filas = await db.RiesgoEmpresa.AsNoTracking()
            .Where(r => r.Estado == EstadoRiesgo.Confirmado)
            .Select(r => r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                .Select(e => new { P = (short?)e.Probabilidad, S = (short?)e.Severidad })
                .FirstOrDefault())
            .ToListAsync(ct);
        return filas.Select(x => (x?.P, x?.S)).ToList();
    }
}
