using Norvia.Application.Features.Empresas.ListarRubrosRegistro.DTOs;

namespace Norvia.Application.Features.Empresas.ListarRubrosRegistro;

public interface IListarRubrosRegistroRepositorio
{
    /// <summary>Rubros activos, ordenados por nombre.</summary>
    Task<List<RubroRegistroResponseDto>> ListarActivosAsync(CancellationToken cancellationToken);
}
