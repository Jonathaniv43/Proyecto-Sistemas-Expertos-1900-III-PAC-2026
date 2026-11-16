using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ActualizarPlantillaDocumento;

public interface IActualizarPlantillaDocumentoRepositorio
{
    Task<PlantillaDocumento?> BuscarAsync(Guid id, CancellationToken ct);
    Task<bool> RubroActivoExisteAsync(short rubroId, CancellationToken ct);
}
