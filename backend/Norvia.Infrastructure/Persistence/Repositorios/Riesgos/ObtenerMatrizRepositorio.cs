using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.ObtenerMatriz;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa, EvaluacionRiesgo y Control son tablas de empresa: el filtro global limita a la empresa de la sesión.
public sealed class ObtenerMatrizRepositorio(NorviaDbContext db) : IObtenerMatrizRepositorio
{
    public async Task<IReadOnlyList<(short? Probabilidad, short? Severidad, bool TieneControles)>> UbicacionesAsync(CancellationToken ct)
    {
        var filas = await db.RiesgoEmpresa.AsNoTracking()
            .Where(r => r.Estado == EstadoRiesgo.Confirmado)
            .Select(r => new
            {
                Evaluacion = r.Evaluaciones.Where(e => e.Tipo == TipoEvaluacion.Inicial)
                    .OrderByDescending(e => e.EvaluadoEn).ThenByDescending(e => e.Id)
                    .Select(e => new { P = (short?)e.Probabilidad, S = (short?)e.Severidad })
                    .FirstOrDefault(),
                TieneControles = r.Controles.Any()
            })
            .ToListAsync(ct);
        return filas.Select(x => (x.Evaluacion?.P, x.Evaluacion?.S, x.TieneControles)).ToList();
    }
}
