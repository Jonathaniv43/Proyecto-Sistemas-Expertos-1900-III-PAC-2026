using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Common.Seguridad;

/// <summary>
/// Ventana anti-spam compartida por los envíos de código de 6 dígitos (RN-86): como máximo
/// <see cref="MaximoEnviosPorVentana"/> correos cada <see cref="MinutosVigencia"/> minutos, para no usar la API
/// como fábrica de correos ajenos. La usan tanto el código de Microsoft como el de activación de cuenta.
/// </summary>
public static class VentanaEnvioCodigo
{
    public const int MinutosVigencia = 10;
    public const int MaximoEnviosPorVentana = 3;
    private static readonly TimeSpan Ventana = TimeSpan.FromMinutes(MinutosVigencia);

    /// <summary>
    /// true si puede enviar (y ya actualizó los contadores de la ventana en <paramref name="fila"/>);
    /// false si ya alcanzó el máximo de la ventana vigente y debe esperar.
    /// </summary>
    public static bool PuedeEnviar(VerificacionCorreo fila, DateTime ahora)
    {
        if (fila.VentanaEnviosExpiraEn <= ahora)
        {
            // Ventana nueva (fila recién creada o la anterior ya venció): un envío abre la cuenta de la ventana.
            fila.EnviosEnVentana = 1;
            fila.VentanaEnviosExpiraEn = ahora.Add(Ventana);
            return true;
        }

        if (fila.EnviosEnVentana >= MaximoEnviosPorVentana) return false;

        fila.EnviosEnVentana++;
        return true;
    }
}
