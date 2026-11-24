using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.EvaluarRiesgoResidual;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa, EvaluacionRiesgo, Control y VinculoEmpresa son tablas de empresa: el filtro global limita a la sesión.
public sealed class EvaluarRiesgoResidualRepositorio(NorviaDbContext db) : IEvaluarRiesgoResidualRepositorio
{
    public async Task<(EstadoRiesgo Estado, bool TieneInicial, bool TieneControles)?> RiesgoAsync(Guid riesgoId, CancellationToken ct)
    {
        var r = await db.RiesgoEmpresa.AsNoTracking().Where(x => x.Id == riesgoId)
            .Select(x => new
            {
                x.Estado,
                TieneInicial = x.Evaluaciones.Any(e => e.Tipo == TipoEvaluacion.Inicial),
                TieneControles = x.Controles.Any()
            })
            .FirstOrDefaultAsync(ct);
        return r is null ? null : (r.Estado, r.TieneInicial, r.TieneControles);
    }

    public async Task<string> NombreDelVinculoAsync(Guid vinculoId, CancellationToken ct) =>
        (await db.VinculoEmpresa.AsNoTracking().Where(v => v.Id == vinculoId).Select(v => v.Persona).SingleAsync(ct)).NombreCompleto;

    public void Agregar(EvaluacionRiesgo evaluacion) => db.EvaluacionRiesgo.Add(evaluacion);
}
