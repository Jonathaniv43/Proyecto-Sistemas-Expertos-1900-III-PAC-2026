using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Notificaciones.MarcarNotificacionLeida;

/// <summary>
/// HU-117: marca como leída una notificación propia. RN-79: solo el destinatario la marca; la de otra persona o de otra
/// empresa responde como inexistente. Marcarla otra vez no cambia la hora en que se leyó por primera vez.
/// </summary>
public sealed class MarcarNotificacionLeidaServicio(
    IMarcarNotificacionLeidaRepositorio repositorio,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(Guid id, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (await repositorio.BuscarPropiaAsync(id, vinculoId, ct) is not { } notificacion) return Errores.NotificacionNoEncontrada;

        if (notificacion.LeidaEn is null)
        {
            notificacion.LeidaEn = await reloj.AhoraAsync(ct);
            await unidadDeTrabajo.GuardarCambiosAsync(ct);
        }
        return Resultado.Exito();
    }
}
