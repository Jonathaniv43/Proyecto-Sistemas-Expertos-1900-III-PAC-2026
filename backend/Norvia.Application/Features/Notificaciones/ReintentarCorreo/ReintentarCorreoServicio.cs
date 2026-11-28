using Norvia.Application.Common.CasosDeUso;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Notificaciones;
using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Notificaciones.ReintentarCorreo;

/// <summary>
/// Botón "Reintentar" de una notificación propia (RN-78): deja su correo pendiente para ya y despierta al proceso
/// de fondo, en vez de esperar su próxima vuelta. Si ya se envió o se está enviando, no hace nada y responde igual
/// (es idempotente).
/// </summary>
public sealed class ReintentarCorreoServicio(
    IReintentarCorreoRepositorio repositorio,
    IAvisoCorreosPendientes avisoCorreos,
    IReloj reloj,
    ITenantActual sesion) : IServicioCasoDeUso
{
    public async Task<Resultado> EjecutarAsync(Guid id, CancellationToken ct)
    {
        if (sesion.VinculoEmpresaId is not Guid vinculoId) return Errores.SinVinculo;
        if (!await repositorio.EsPropiaAsync(id, vinculoId, ct)) return Errores.NotificacionNoEncontrada;

        await repositorio.ProgramarYaAsync(id, await reloj.AhoraAsync(ct), ct);
        avisoCorreos.Avisar();
        return Resultado.Exito();
    }
}
