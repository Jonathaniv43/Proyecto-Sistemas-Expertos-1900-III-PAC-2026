namespace Norvia.Application.Common.Correo;

public interface IEnvioCorreo
{
    /// <summary>Intenta enviar el mensaje una vez. No lanza: si falla, el resultado trae el motivo.</summary>
    Task<ResultadoEnvioCorreo> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancellationToken);
}

/// <summary>Resultado de un intento de envío.</summary>
/// <param name="Enviado">true si el servidor SMTP aceptó el mensaje.</param>
/// <param name="Error">Motivo del fallo (para registrar el intento, HU-118); nulo si se envió.</param>
public sealed record ResultadoEnvioCorreo(bool Enviado, string? Error = null)
{
    public static readonly ResultadoEnvioCorreo Exito = new(true);
    public static ResultadoEnvioCorreo Fallo(string error) => new(false, error);
}
