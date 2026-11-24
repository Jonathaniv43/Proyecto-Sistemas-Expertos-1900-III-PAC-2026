using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ActualizarPlantillaTarea;

public interface IActualizarPlantillaTareaRepositorio
{
    Task<PlantillaTarea?> BuscarAsync(Guid id, CancellationToken ct);
}
