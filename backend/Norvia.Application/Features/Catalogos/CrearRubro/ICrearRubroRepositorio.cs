using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Application.Features.Catalogos.CrearRubro;

public interface ICrearRubroRepositorio
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct);
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct);

    /// <summary>El Id de Rubro no es autogenerado (los primeros vienen del seed de HU-097).</summary>
    Task<short> MayorIdAsync(CancellationToken ct);

    void Agregar(Rubro rubro);
}
