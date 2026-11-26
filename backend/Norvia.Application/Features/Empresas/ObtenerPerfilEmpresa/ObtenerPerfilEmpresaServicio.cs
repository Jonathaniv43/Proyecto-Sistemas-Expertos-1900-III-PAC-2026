using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Empresas.DTOs;

namespace Norvia.Application.Features.Empresas.ObtenerPerfilEmpresa;

/// <summary>Devuelve el perfil de la empresa de la sesión (HU-026).</summary>
public sealed class ObtenerPerfilEmpresaServicio(
    IObtenerPerfilEmpresaRepositorio repositorio,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<PerfilEmpresaResponseDto>> EjecutarAsync(CancellationToken ct)
    {
        if (sesion.EmpresaId is not Guid empresaId) return Errores.EmpresaNoEncontrada;

        var empresa = await repositorio.BuscarAsync(empresaId, ct);
        return empresa is null ? Errores.EmpresaNoEncontrada : PerfilEmpresaResponseDto.De(empresa);
    }
}
