using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearPeligro;

public interface ICrearPeligroRepositorio
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct);
    void Agregar(Peligro peligro);
}
