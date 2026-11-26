using Norvia.Application.Features.Plan.Propuesta;

namespace Norvia.Application.Features.Plan.ObtenerPropuestaPlan;

public interface IObtenerPropuestaPlanRepositorio
{
    Task<DatosPropuestaPlan> DatosAsync(CancellationToken ct);
}
