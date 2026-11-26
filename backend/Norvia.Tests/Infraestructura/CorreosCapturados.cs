using System.Collections.Concurrent;
using Norvia.Application.Common.Correo;

namespace Norvia.Tests.Infraestructura;

/// <summary>
/// Reemplaza el envío SMTP en las pruebas: guarda cada mensaje en memoria para leerlo (por ejemplo, el
/// enlace de cambio de contraseña de HU-022). Nada sale del proceso.
/// <para>Con <see cref="Fallar"/> simula un servidor caído (reintentos de HU-118).</para>
/// </summary>
public sealed class CorreosCapturados : IEnvioCorreo
{
    private readonly ConcurrentQueue<MensajeCorreo> _mensajes = new();

    /// <summary>Mientras sea true, todo envío falla y no se guarda.</summary>
    public bool Fallar { get; set; }

    public Task<ResultadoEnvioCorreo> EnviarAsync(MensajeCorreo mensaje, CancellationToken cancellationToken)
    {
        if (Fallar) return Task.FromResult(ResultadoEnvioCorreo.Fallo("SMTP de prueba caído"));
        _mensajes.Enqueue(mensaje);
        return Task.FromResult(ResultadoEnvioCorreo.Exito);
    }

    public IReadOnlyList<MensajeCorreo> Para(string destino) =>
        _mensajes.Where(m => string.Equals(m.Destino, destino, StringComparison.OrdinalIgnoreCase)).ToList();
}
