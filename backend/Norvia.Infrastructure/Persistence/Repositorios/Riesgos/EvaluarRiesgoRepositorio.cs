using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Riesgos.EvaluarRiesgo;
using Norvia.Domain.Entities.Riesgos;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Riesgos;

// RiesgoEmpresa, EvaluacionRiesgo, VinculoEmpresa y Accidente son tablas de empresa: el filtro global limita a la sesión.
public sealed class EvaluarRiesgoRepositorio(NorviaDbContext db) : IEvaluarRiesgoRepositorio
{
    public async Task<EstadoRiesgo?> EstadoDelRiesgoAsync(Guid riesgoId, CancellationToken ct) =>
        await db.RiesgoEmpresa.AsNoTracking().Where(r => r.Id == riesgoId).Select(r => (EstadoRiesgo?)r.Estado).FirstOrDefaultAsync(ct);

    public async Task<string> NombreDelVinculoAsync(Guid vinculoId, CancellationToken ct) =>
        (await db.VinculoEmpresa.AsNoTracking().Where(v => v.Id == vinculoId).Select(v => v.Persona).SingleAsync(ct)).NombreCompleto;

    public Task<bool> EventoEnlazableAsync(Guid accidenteId, CancellationToken ct) =>
        db.Accidente.AnyAsync(a => a.Id == accidenteId && a.Estado != EstadoAccidente.Descartado, ct);

    public void Agregar(EvaluacionRiesgo evaluacion) => db.EvaluacionRiesgo.Add(evaluacion);
}
