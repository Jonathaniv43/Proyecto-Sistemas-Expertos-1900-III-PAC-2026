using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CambiarEstadoRubro;

public interface ICambiarEstadoRubroRepositorio
{
    Task<Rubro?> BuscarAsync(short id, CancellationToken ct);
}
