using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ActualizarCategoriaRiesgo;

public interface IActualizarCategoriaRiesgoRepositorio
{
    Task<CategoriaRiesgo?> BuscarAsync(Guid id, CancellationToken ct);
    Task<bool> RubroActivoExisteAsync(short rubroId, CancellationToken ct);
    Task<bool> ExisteNombreEnOtraAsync(Guid id, short rubroId, string nombre, CancellationToken ct);
}
