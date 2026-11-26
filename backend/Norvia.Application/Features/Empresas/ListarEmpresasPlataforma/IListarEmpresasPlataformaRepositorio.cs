using Norvia.Application.Features.Empresas.ListarEmpresasPlataforma.DTOs;

namespace Norvia.Application.Features.Empresas.ListarEmpresasPlataforma;

public interface IListarEmpresasPlataformaRepositorio
{
    /// <summary>Empresas registradas ordenadas de la más reciente a la primera; <paramref name="buscar"/> filtra por razón social o rubro.</summary>
    Task<IReadOnlyList<EmpresaResumenResponseDto>> ListarAsync(string? buscar, CancellationToken ct);
}
