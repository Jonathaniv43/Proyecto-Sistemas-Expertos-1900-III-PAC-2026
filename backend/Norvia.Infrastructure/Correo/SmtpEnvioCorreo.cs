using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Norvia.Application.Common.Correo;

namespace Norvia.Infrastructure.Correo;

public sealed class SmtpEnvioCorreo(
    IOptions<CorreoOpciones> opciones,
    ILogger<SmtpEnvioCorreo> log) : IEnvioCorreo
{
    public async Task<ResultadoEnvioCorreo> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancellationToken)
    {
        var cfg = opciones.Value;
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(cfg.RemitenteNombre, cfg.Remitente));
            email.To.Add(MailboxAddress.Parse(mensaje.Destino));
            email.Subject = mensaje.Asunto;
            email.Body = new TextPart("html") { Text = mensaje.CuerpoHtml };

            using var cliente = new SmtpClient();
            await cliente.ConnectAsync(cfg.Host, cfg.Port, cfg.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);

            if (!string.IsNullOrWhiteSpace(cfg.Usuario))
                await cliente.AuthenticateAsync(cfg.Usuario, cfg.Password, cancellationToken);

            await cliente.SendAsync(email, cancellationToken);
            await cliente.DisconnectAsync(true, cancellationToken);
            return ResultadoEnvioCorreo.Exito;
        }
        // Si se canceló (la API se está apagando), no cuenta como intento fallido.
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            log.LogError(ex, "No se pudo enviar correo a {Destino}", mensaje.Destino);
            return ResultadoEnvioCorreo.Fallo($"{ex.GetType().Name}: {ex.Message}");
        }
    }
}