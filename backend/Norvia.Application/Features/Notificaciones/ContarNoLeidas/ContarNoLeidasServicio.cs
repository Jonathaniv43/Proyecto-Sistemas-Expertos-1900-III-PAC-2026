using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Notificaciones.DTOs;

namespace Norvia.Application.Features.Notificaciones.ContarNoLeidas;

/// <summary>
/// HU-117: cuántas notificaciones no ha leído quien tiene la sesión. Es liviano a propósito: el encabezado lo consulta
/// cada cierto tiempo para la campana, sin traer la lista (RN-79: solo las suyas).
/// </summary>
public sealed class ContarNoLeidasServicio(IContarNoLeidasRepositorio repositorio, ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado<NoLeidasResponseDto>> EjecutarAsync(CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        return new NoLeidasResponseDto(await repositorio.ContarAsync(vinculoId, ct));
    }
}
