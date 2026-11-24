using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ListarPlantillasTarea;

public interface IListarPlantillasTareaRepositorio
{
    /// <summary>Todas, o solo las de un peligro; incluye las desactivadas.</summary>
    Task<List<PlantillaTarea>> ListarAsync(Guid? peligroId, CancellationToken ct);
}
