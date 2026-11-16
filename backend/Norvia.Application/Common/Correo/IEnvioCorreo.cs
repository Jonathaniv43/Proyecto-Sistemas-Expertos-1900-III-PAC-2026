namespace Norvia.Application.Common.Correo;

public interface IEnvioCorreo
{
    /// <returns>true si el servidor SMTP aceptó el mensaje.</returns>
    Task<bool> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancellationToken);
}