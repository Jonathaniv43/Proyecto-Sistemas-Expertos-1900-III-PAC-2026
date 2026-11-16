using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn.DTOs;

namespace Norvia.Application.Features.Empresas.ResolverEmpresaPorRtn;

/// <summary>
/// Resuelve el identificador de empresa cifrado a partir del RTN (RN-01, landing → login).
/// Solo expone datos públicos; no otorga sesión.
/// </summary>
public sealed class ResolverEmpresaPorRtnServicio(
    IResolverEmpresaPorRtnRepositorio repositorio,
    ICifradorEmpresaId cifrador) : IServicioCasoDeUso
{
    public async Task<Resultado<ResolverEmpresaPorRtnResponseDto>> EjecutarAsync(
        ResolverEmpresaPorRtnRequestDto request, CancellationToken cancellationToken)
    {
        var rtn = request.Rtn.Trim();
        var empresa = await repositorio.BuscarActivaPorRtnAsync(rtn, cancellationToken);
        if (empresa is null) return Errores.EmpresaPorRtnNoEncontrada;

        var idEmpresa = cifrador.Cifrar(empresa.Id);
        return new ResolverEmpresaPorRtnResponseDto(idEmpresa, empresa.RazonSocial, empresa.Rtn);
    }
}