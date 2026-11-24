using Norvia.Application.Features.Plan.ObtenerPropuestaPlan;
using Norvia.Application.Features.Plan.Propuesta;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence.Repositorios.Plan;

public sealed class ObtenerPropuestaPlanRepositorio(NorviaDbContext db) : IObtenerPropuestaPlanRepositorio
{
    public Task<DatosPropuestaPlan> DatosAsync(CancellationToken ct) => ConsultaPropuestaPlan.DatosAsync(db, ct);
}
