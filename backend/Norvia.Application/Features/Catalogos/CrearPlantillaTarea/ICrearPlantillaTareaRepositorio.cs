using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearPlantillaTarea;

public interface ICrearPlantillaTareaRepositorio
{
    Task<bool> PeligroActivoAsync(Guid peligroId, CancellationToken ct);
    void Agregar(PlantillaTarea plantilla);
}
