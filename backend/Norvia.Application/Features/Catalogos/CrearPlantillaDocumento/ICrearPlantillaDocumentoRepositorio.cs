using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearPlantillaDocumento;

public interface ICrearPlantillaDocumentoRepositorio
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct);
    Task<bool> RubroActivoExisteAsync(short rubroId, CancellationToken ct);
    void Agregar(PlantillaDocumento plantilla);
}
