using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Common.Seguridad;

/// <summary>
/// Ventana anti-spam compartida por los envíos de código de 6 dígitos (RN-86): como máximo
/// <see cref="MaximoEnviosPorVentana"/> correos cada <see cref="MinutosVigencia"/> minutos, para no usar la API
/// como fábrica de correos ajenos. La usan tanto el código de Microsoft como el de activación de cuenta.
/// </summary>
public static class VentanaEnvioCodigo
{
    /// <summary>Vigencia del código cuando la persona lo pidió y lo está esperando, y duración de la ventana de envíos.</summary>
    public const int MinutosVigencia = 10;
    public const int MaximoEnviosPorVentana = 3;

    /// <summary>
    /// Vigencia del código de activación que se manda al dar de alta a otra persona (vínculo o encuestado, RN-87):
    /// le llega un correo que no esperaba y puede abrirlo horas después. Un reenvío lo pide la propia persona,
    /// que está frente a la pantalla, y vuelve a la vigencia corta.
    /// </summary>
    public static readonly TimeSpan VigenciaActivacionPorAlta = TimeSpan.FromHours(48);
    public const string TextoVigenciaActivacionPorAlta = "48 horas";
    public static readonly string TextoVigenciaCorta = $"unos {MinutosVigencia} minutos";
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
