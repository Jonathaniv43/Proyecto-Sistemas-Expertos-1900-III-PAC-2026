using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Correo;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Entities.Notificaciones;
using Norvia.Domain.Enums;
using Norvia.Infrastructure.Data;
using Norvia.Infrastructure.Opciones;

namespace Norvia.Infrastructure.Notificaciones;

/// <summary>
/// Bandeja de salida del correo (HU-118, RN-78, D28): envía los correos pendientes de las notificaciones y reintenta
/// los que fallaron, con una espera que crece en cada intento. Cada intento queda registrado en la fila (intentos,
/// último error) y en el log. Al agotar los intentos, el correo queda "fallido"; la notificación sigue visible en
/// Norvia y la operación que la originó no se toca.
/// <para>Lo llama cada cierto tiempo <c>ProcesadorCorreosPendientes</c> (API); las pruebas lo llaman directo.</para>
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

    /// <summary>Procesa un lote de correos vencidos. Devuelve cuántos intentó.</summary>
    public async Task<int> ProcesarAsync(CancellationToken ct)
    {
        var cfg = opciones.Value;
        var ahora = await reloj.AhoraAsync(ct);

        // Proceso de plataforma, sin sesión de empresa: recorre los pendientes de todas las empresas.
        // Cada fila se envía solo a su destinatario, que es un vínculo de la misma empresa (FK compuesta).
        var lote = await db.Notificacion.IgnoreQueryFilters()
            .Include(n => n.Destinatario).ThenInclude(v => v.Acceso)
            .Include(n => n.Destinatario).ThenInclude(v => v.Persona)
            .Where(n => n.CorreoEstado == EstadoCorreo.Pendiente
                        && (n.CorreoProximoIntentoEn == null || n.CorreoProximoIntentoEn <= ahora))
            .OrderBy(n => n.CorreoProximoIntentoEn ?? n.CreadaEn)
            .ThenBy(n => n.Id)
            .Take(cfg.LoteMaximo)
            .ToListAsync(ct);

        foreach (var notificacion in lote)
        {
            await IntentarAsync(notificacion, cfg, ct);
            // Se guarda cada intento: si la API se apaga a mitad del lote, no se reenvía lo ya enviado.
            await db.SaveChangesAsync(ct);
        }

        return lote.Count;
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

        return await envioCorreo.EnviarAsync(
            new MensajeCorreo(n.Destinatario.Acceso.Email, renderizado.Asunto, renderizado.CuerpoHtml), ct);
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
