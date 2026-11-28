using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Correo;
using Norvia.Application.Common.Interfaces;
using Norvia.Application.Common.Notificaciones;
using Norvia.Domain.Entities.Notificaciones;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Notificaciones;

/// <summary>
/// Envía el correo de las notificaciones y reintenta los que fallaron, con una espera que crece en cada intento
/// (HU-118, RN-78, D28). Cada intento queda registrado en la fila (intentos, último error) y en el log. Al agotar
/// los intentos, el correo queda "fallido"; la notificación sigue visible en Norvia y la operación que la originó
/// no se toca.
/// <para>
/// Solo el proceso de fondo (<c>ProcesadorCorreosPendientes</c>) llama a este servicio; la API únicamente lo
/// despierta (<see cref="SenalCorreosPendientes"/>). Tomar una fila la pasa a <c>en_proceso</c> con un plazo
/// (<see cref="PlazoEnProceso"/>) en <c>CorreoProximoIntentoEn</c>: si el proceso se interrumpe a mitad (reinicio
/// del servidor, caída de la base), al vencer el plazo la fila se vuelve a tomar en vez de quedar trabada. Para
/// que ese reintento no duplique un correo que sí salió, cada intento lleva una clave de idempotencia.
/// </para>
/// </summary>
public sealed class EnvioNotificacionesPendientes(
    NorviaDbContext db,
    IReloj reloj,
    IPlantillasCorreo plantillas,
    IEnvioCorreo envioCorreo,
    IOptions<NotificacionesOpciones> opciones,
    IOptions<CorreoOpciones> correo,
    ILogger<EnvioNotificacionesPendientes> log)
{
    private const int LargoMaximoError = 1000;

    /// <summary>Más que el timeout de la llamada al proveedor de correo, para no retomar una fila que sigue en curso.</summary>
    public static readonly TimeSpan PlazoEnProceso = TimeSpan.FromMinutes(2);

    /// <summary>Procesa un lote de correos vencidos (poller de fondo). Devuelve cuántos intentó.</summary>
    public async Task<int> ProcesarAsync(CancellationToken ct)
    {
        var cfg = opciones.Value;
        var ahora = await reloj.AhoraAsync(ct);

        // Proceso de plataforma, sin sesión de empresa: recorre los pendientes de todas las empresas.
        // Pendientes cuya espera venció, y en_proceso cuyo plazo venció (un intento que se interrumpió).
        var ids = await db.Notificacion.IgnoreQueryFilters()
            .Where(n => (n.CorreoEstado == EstadoCorreo.Pendiente
                         && (n.CorreoProximoIntentoEn == null || n.CorreoProximoIntentoEn <= ahora))
                        || (n.CorreoEstado == EstadoCorreo.EnProceso && n.CorreoProximoIntentoEn <= ahora))
            .OrderBy(n => n.CorreoProximoIntentoEn ?? n.CreadaEn)
            .ThenBy(n => n.Id)
            .Take(cfg.LoteMaximo)
            .Select(n => n.Id)
            .ToListAsync(ct);

        var procesados = 0;
        foreach (var id in ids)
        {
            if (await IntentarUnoAsync(id, cfg, ct)) procesados++;
        }
        return procesados;
    }

    /// <summary>Toma la fila (guardo atómico con plazo), intenta el envío y guarda. false si ya no estaba disponible.</summary>
    private async Task<bool> IntentarUnoAsync(Guid id, NotificacionesOpciones cfg, CancellationToken ct)
    {
        var ahora = await reloj.AhoraAsync(ct);
        var plazo = ahora + PlazoEnProceso;
        var tomada = await db.Notificacion.IgnoreQueryFilters()
            .Where(n => n.Id == id
                        && (n.CorreoEstado == EstadoCorreo.Pendiente
                            || (n.CorreoEstado == EstadoCorreo.EnProceso && n.CorreoProximoIntentoEn <= ahora)))
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.CorreoEstado, EstadoCorreo.EnProceso)
                .SetProperty(n => n.CorreoProximoIntentoEn, plazo), ct);
        if (tomada == 0) return false;

        var n = await db.Notificacion.IgnoreQueryFilters()
            .Include(x => x.Destinatario).ThenInclude(v => v.Acceso)
            .Include(x => x.Destinatario).ThenInclude(v => v.Persona)
            .FirstAsync(x => x.Id == id, ct);

        // Si esta fila ya estaba rastreada en este mismo DbContext (p. ej. el intento inmediato, en el mismo
        // scope que la creó), la consulta anterior devuelve esa instancia por resolución de identidad, con sus
        // valores "originales" desactualizados: el UPDATE atómico de arriba no pasa por el rastreador. Sin este
        // reload, un valor final que coincida con ese original desactualizado (p. ej. volver a "pendiente") no
        // se seguiría viendo como un cambio y la fila quedaría en "en_proceso" para siempre.
        await db.Entry(n).ReloadAsync(ct);

        await IntentarAsync(n, cfg, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task IntentarAsync(Notificacion n, NotificacionesOpciones cfg, CancellationToken ct)
    {
        if (n.Destinatario.Estado != EstadoVinculo.Activo)
        {
            // Ya no tiene acceso a la empresa: no se le escribe. En Norvia la notificación queda como estaba.
            MarcarFallido(n, "El destinatario ya no tiene acceso vigente a la empresa.");
            log.LogInformation("Correo de la notificación {Id} descartado: el vínculo no está activo.", n.Id);
            return;
        }

        var resultado = await EnviarAsync(n, ct);
        n.CorreoIntentos++;
        var ahora = await reloj.AhoraAsync(ct);

        if (resultado.Enviado)
        {
            n.CorreoEstado = EstadoCorreo.Enviado;
            n.CorreoEnviadoEn = ahora;
            n.CorreoProximoIntentoEn = null;
            n.CorreoUltimoError = null;
            log.LogInformation("Correo de la notificación {Id} enviado (intento {Intento}).", n.Id, n.CorreoIntentos);
            return;
        }

        var error = Recortar(resultado.Error ?? "Error desconocido.");
        if (n.CorreoIntentos >= cfg.MaxIntentos)
        {
            MarcarFallido(n, error);
            log.LogWarning("Correo de la notificación {Id} fallido tras {Intentos} intentos: {Error}", n.Id, n.CorreoIntentos, error);
            return;
        }

        n.CorreoEstado = EstadoCorreo.Pendiente;
        n.CorreoUltimoError = error;
        n.CorreoProximoIntentoEn = ahora + cfg.EsperaTras(n.CorreoIntentos);
        log.LogWarning("Correo de la notificación {Id} falló (intento {Intento} de {Max}); se reintenta desde {Proximo:o}: {Error}",
            n.Id, n.CorreoIntentos, cfg.MaxIntentos, n.CorreoProximoIntentoEn, error);
    }

    private async Task<ResultadoEnvioCorreo> EnviarAsync(Notificacion n, CancellationToken ct)
    {
        static string Html(string texto) => WebUtility.HtmlEncode(texto);
        var variables = new Dictionary<string, string>
        {
            ["Nombre"] = Html(n.Destinatario.Persona.PrimerNombre),
            ["Titulo"] = Html(n.Titulo),
            ["Mensaje"] = Html(n.Mensaje),
            // El asunto es texto plano: va sin codificar.
            ["Asunto"] = n.Titulo,
            ["Url"] = Html(correo.Value.UrlBaseFrontend.TrimEnd('/') + RutaDe(n))
        };
        var renderizado = await plantillas.RenderizarAsync(CodigosPlantillaCorreo.Notificacion, variables, ct);
        if (renderizado is null)
            return ResultadoEnvioCorreo.Fallo($"Falta la plantilla de correo '{CodigosPlantillaCorreo.Notificacion}'.");

        // Mismo número de intento si un envío se interrumpió antes de guardarse: el proveedor no lo duplica.
        var clave = $"notificacion-{n.Id}-{n.CorreoIntentos}";
        return await envioCorreo.EnviarAsync(
            new MensajeCorreo(n.Destinatario.Acceso.Email, renderizado.Asunto, renderizado.CuerpoHtml, clave), ct);
    }

    /// <summary>Pantalla del front que abre el correo. La de la política se fija con HU-116 (ver la versión vigente).</summary>
    private static string RutaDe(Notificacion n) => n.Tipo switch
    {
        TipoNotificacion.TareaAsignada => "/mis-tareas",
        _ => "/notificaciones"
    };

    private static void MarcarFallido(Notificacion n, string error)
    {
        n.CorreoEstado = EstadoCorreo.Fallido;
        n.CorreoProximoIntentoEn = null;
        n.CorreoUltimoError = Recortar(error);
    }

    private static string Recortar(string error) => error.Length <= LargoMaximoError ? error : error[..LargoMaximoError];
}
