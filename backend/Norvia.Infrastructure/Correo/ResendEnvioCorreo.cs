using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Norvia.Application.Common.Correo;

namespace Norvia.Infrastructure.Correo;

public sealed class ResendEnvioCorreo(
    HttpClient http,
    IOptions<CorreoOpciones> opciones,
    ILogger<ResendEnvioCorreo> log) : IEnvioCorreo
{
    public async Task<ResultadoEnvioCorreo> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancellationToken)
    {
        var cfg = opciones.Value;
        var payload = new
        {
            from = $"{cfg.RemitenteNombre} <{cfg.Remitente}>",
            to = new[] { mensaje.Destino },
            subject = mensaje.Asunto,
            html = mensaje.CuerpoHtml,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails") { Content = JsonContent.Create(payload) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cfg.ApiKeyResend);

        try
        {
            using var respuesta = await http.SendAsync(request, cancellationToken);
            if (respuesta.IsSuccessStatusCode)
                return ResultadoEnvioCorreo.Exito;

            var cuerpo = await respuesta.Content.ReadAsStringAsync(cancellationToken);
            log.LogError("Resend rechazó el correo a {Destino}: {Status} {Cuerpo}", mensaje.Destino, respuesta.StatusCode, cuerpo);
            return ResultadoEnvioCorreo.Fallo($"Resend {(int)respuesta.StatusCode}: {cuerpo}");
        }
        // Si se canceló (la API se está apagando), no cuenta como intento fallido.
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            log.LogError(ex, "No se pudo enviar correo a {Destino} via Resend", mensaje.Destino);
            return ResultadoEnvioCorreo.Fallo($"{ex.GetType().Name}: {ex.Message}");
        }
    }
}
