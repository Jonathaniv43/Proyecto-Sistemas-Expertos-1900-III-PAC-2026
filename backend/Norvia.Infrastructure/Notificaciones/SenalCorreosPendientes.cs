using System.Threading.Channels;
using Norvia.Application.Common.Notificaciones;

namespace Norvia.Infrastructure.Notificaciones;

/// <summary>
/// Señal en memoria entre la API y el proceso de fondo. Varios avisos seguidos se juntan en uno: basta con que
/// el proceso de fondo despierte una vez para vaciar todo lo pendiente.
/// </summary>
public sealed class SenalCorreosPendientes : IAvisoCorreosPendientes
{
    private readonly Channel<byte> _canal = Channel.CreateBounded<byte>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

    public void Avisar() => _canal.Writer.TryWrite(0);

    /// <summary>Espera un aviso o, como máximo, <paramref name="maximo"/>.</summary>
    public async Task EsperarAsync(TimeSpan maximo, CancellationToken ct)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limite.CancelAfter(maximo);
        try
        {
            await _canal.Reader.ReadAsync(limite.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // Pasó el intervalo sin avisos: toca la vuelta periódica.
        }
    }
}
