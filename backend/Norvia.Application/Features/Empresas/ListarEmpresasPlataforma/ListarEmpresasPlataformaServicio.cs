using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Empresas.ListarEmpresasPlataforma.DTOs;

namespace Norvia.Application.Features.Empresas.ListarEmpresasPlataforma;

/// <summary>
/// HU-024: listado de empresas registradas y cantidad de vínculos vigentes para el staff de plataforma,
/// sin incluir datos de SST (RN-05).
/// </summary>
public sealed class ListarEmpresasPlataformaServicio(IListarEmpresasPlataformaRepositorio repositorio) : IServicioCasoDeUso
{
    public async Task<Resultado<IReadOnlyList<EmpresaResumenResponseDto>>> EjecutarAsync(
        FiltrosEmpresasPlataformaDto filtros, CancellationToken ct)
    {
        var buscar = string.IsNullOrWhiteSpace(filtros.Buscar) ? null : filtros.Buscar.Trim();
        var lista = await repositorio.ListarAsync(buscar, ct);
        return Resultado<IReadOnlyList<EmpresaResumenResponseDto>>.Exito(lista);
    }
}
