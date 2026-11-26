using Microsoft.Extensions.Options;
using Norvia.Infrastructure.Notificaciones;
using Norvia.Infrastructure.Opciones;

namespace Norvia.API.SegundoPlano;

/// <summary>
/// HU-118: revisa la bandeja de salida cada <see cref="NotificacionesOpciones.IntervaloSegundos"/> y envía los correos
/// pendientes con <see cref="EnvioNotificacionesPendientes"/>, que también lleva los reintentos. Con
/// <c>Notificaciones__Habilitado=false</c> no hace nada (las pruebas procesan la bandeja a mano).
/// <para>
/// Si la API está dormida (hosting gratuito sin tráfico), no envía; los pendientes salen al despertar, en orden.
/// </para>
/// </summary>
public sealed class ProcesadorCorreosPendientes(
    IServiceScopeFactory scopes,
    IOptions<NotificacionesOpciones> opciones,
    ILogger<ProcesadorCorreosPendientes> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = opciones.Value;
        if (!cfg.Habilitado)
        {
            log.LogInformation("Envío de correos de notificaciones desactivado (Notificaciones__Habilitado=false).");
            return;
        }

        using var temporizador = new PeriodicTimer(TimeSpan.FromSeconds(cfg.IntervaloSegundos));
        do
        {
            await ProcesarAsync(cfg, stoppingToken);
        }
        while (await EsperarAsync(temporizador, stoppingToken));
    }

    /// <summary>Vacía lo vencido: si el lote salió lleno, puede haber más esperando y sigue sin aguardar al intervalo.</summary>
    private async Task ProcesarAsync(NotificacionesOpciones cfg, CancellationToken ct)
    {
        try
        {
            int procesados;
            do
            {
                // Un scope por lote: DbContext y repositorios son scoped, y así no crecen con el tiempo.
                using var scope = scopes.CreateScope();
                var envio = scope.ServiceProvider.GetRequiredService<EnvioNotificacionesPendientes>();
                procesados = await envio.ProcesarAsync(ct);
            }
            while (procesados >= cfg.LoteMaximo && !ct.IsCancellationRequested);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // La API se está apagando.
        }
        catch (Exception ex)
        {
            // Por ejemplo, la base no responde: se reintenta en la próxima vuelta sin tumbar la API.
            log.LogError(ex, "No se pudo procesar la bandeja de correos de notificaciones.");
        }
    }

    private static async Task<bool> EsperarAsync(PeriodicTimer temporizador, CancellationToken ct)
    {
        try
        {
            return await temporizador.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
