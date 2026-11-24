using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;
using Norvia.Application.Features.Notificaciones.DTOs;
using Norvia.Application.Features.Notificaciones.ListarMisNotificaciones.DTOs;

namespace Norvia.Application.Features.Notificaciones.ListarMisNotificaciones;

/// <summary>
/// HU-117: las notificaciones de quien tiene la sesión, de la más reciente a la más antigua, con el número de no leídas.
/// RN-79: solo las dirigidas a su vínculo (del token), y el filtro global las limita a la empresa de la sesión.
/// </summary>
public sealed class ListarMisNotificacionesServicio(IListarMisNotificacionesRepositorio repositorio, ITenantActual sesion)
    : IServicioCasoDeUso
{
    public async Task<Resultado<MisNotificacionesResponseDto>> EjecutarAsync(FiltrosNotificacionesDto filtros, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;

        var limite = filtros.Limite ?? FiltrosNotificacionesDto.LimitePorDefecto;
        var notificaciones = await repositorio.ListarAsync(vinculoId, limite, ct);
        var noLeidas = await repositorio.ContarNoLeidasAsync(vinculoId, ct);
        return new MisNotificacionesResponseDto(noLeidas, notificaciones);
    }
}
