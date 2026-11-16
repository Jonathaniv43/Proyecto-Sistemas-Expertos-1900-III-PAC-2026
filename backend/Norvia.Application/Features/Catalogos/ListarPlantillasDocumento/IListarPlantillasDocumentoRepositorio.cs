using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ListarPlantillasDocumento;

public interface IListarPlantillasDocumentoRepositorio
{
    Task<List<PlantillaDocumento>> ListarAsync(CancellationToken ct);
}
