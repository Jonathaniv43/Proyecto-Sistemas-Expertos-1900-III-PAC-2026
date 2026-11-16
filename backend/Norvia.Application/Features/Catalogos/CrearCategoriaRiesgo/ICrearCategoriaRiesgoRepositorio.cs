using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearCategoriaRiesgo;

public interface ICrearCategoriaRiesgoRepositorio
{
    Task<bool> RubroActivoExisteAsync(short rubroId, CancellationToken ct);
    Task<bool> ExisteNombreAsync(short rubroId, string nombre, CancellationToken ct);
    void Agregar(CategoriaRiesgo categoria);
}
