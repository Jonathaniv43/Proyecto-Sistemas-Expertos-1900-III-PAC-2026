using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaTarea;

public interface ICambiarEstadoPlantillaTareaRepositorio
{
    Task<PlantillaTarea?> BuscarAsync(Guid id, CancellationToken ct);
}
