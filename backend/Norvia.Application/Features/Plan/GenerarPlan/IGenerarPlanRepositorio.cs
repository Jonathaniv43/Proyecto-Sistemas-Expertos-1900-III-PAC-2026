using Norvia.Application.Features.Plan.Propuesta;
using Norvia.Domain.Entities.Plan;

namespace Norvia.Application.Features.Plan.GenerarPlan;

public interface IGenerarPlanRepositorio
{
    Task<DatosPropuestaPlan> DatosAsync(CancellationToken ct);

    void Agregar(IEnumerable<Tarea> tareas);
}
