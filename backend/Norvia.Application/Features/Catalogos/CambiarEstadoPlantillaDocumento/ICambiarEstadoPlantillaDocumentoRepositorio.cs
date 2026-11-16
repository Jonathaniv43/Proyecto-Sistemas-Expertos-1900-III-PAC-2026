using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoPlantillaDocumento;

public interface ICambiarEstadoPlantillaDocumentoRepositorio
{
    Task<PlantillaDocumento?> BuscarAsync(Guid id, CancellationToken ct);
}
