using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Notificaciones;
using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Notificaciones.ReintentarCorreo;

/// <summary>
/// Botón "Reintentar" de una notificación propia: dispara de inmediato el mismo intento de envío que el poller
/// de fondo, en vez de esperar su próxima vuelta (RN-78). Si el correo ya se envió o alguien más la está
/// procesando en este instante, no hace nada y responde igual de bien (es idempotente).
/// </summary>
public sealed class ReintentarCorreoServicio(
    IReintentarCorreoRepositorio repositorio,
    IEnvioInmediatoNotificacion envioInmediato,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(Guid id, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (!await repositorio.EsPropiaAsync(id, vinculoId, ct)) return Errores.NotificacionNoEncontrada;

        await envioInmediato.IntentarAsync(id, ct);
        return Resultado.Exito();
    }
}
