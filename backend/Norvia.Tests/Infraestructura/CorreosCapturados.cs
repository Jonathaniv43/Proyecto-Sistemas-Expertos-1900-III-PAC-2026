using System.Collections.Concurrent;
using Norvia.Application.Common.Correo;

namespace Norvia.Tests.Infraestructura;

/// <summary>
/// Reemplaza el envío SMTP en las pruebas: guarda cada mensaje en memoria para leerlo (por ejemplo, el
/// enlace de cambio de contraseña de HU-022). Nada sale del proceso.
/// </summary>
public sealed class CorreosCapturados : IEnvioCorreo
{
    private readonly ConcurrentQueue<MensajeCorreo> _mensajes = new();

    public Task<bool> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancellationToken)
    {
        _mensajes.Enqueue(mensaje);
        return Task.FromResult(true);
    }

    public IReadOnlyList<MensajeCorreo> Para(string destino) =>
        _mensajes.Where(m => string.Equals(m.Destino, destino, StringComparison.OrdinalIgnoreCase)).ToList();
}
