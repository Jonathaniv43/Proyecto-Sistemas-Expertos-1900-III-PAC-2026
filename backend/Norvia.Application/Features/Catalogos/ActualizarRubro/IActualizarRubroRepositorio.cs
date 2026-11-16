using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.ActualizarRubro;

public interface IActualizarRubroRepositorio
{
    Task<Rubro?> BuscarAsync(short id, CancellationToken ct);
    Task<bool> ExisteNombreEnOtroAsync(short id, string nombre, CancellationToken ct);
}
