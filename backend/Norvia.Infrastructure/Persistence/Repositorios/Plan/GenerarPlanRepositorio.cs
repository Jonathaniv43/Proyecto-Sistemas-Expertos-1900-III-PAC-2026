using Norvia.Application.Features.Plan.GenerarPlan;
using Norvia.Application.Features.Plan.Propuesta;
using Norvia.Domain.Entities.Plan;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

public sealed class GenerarPlanRepositorio(NorviaDbContext db) : IGenerarPlanRepositorio
{
    public Task<DatosPropuestaPlan> DatosAsync(CancellationToken ct) => ConsultaPropuestaPlan.DatosAsync(db, ct);

    public void Agregar(IEnumerable<Tarea> tareas) => db.Tarea.AddRange(tareas);
}
