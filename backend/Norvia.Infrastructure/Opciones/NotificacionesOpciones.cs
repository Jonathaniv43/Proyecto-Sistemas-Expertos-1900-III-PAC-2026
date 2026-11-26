namespace Norvia.Infrastructure.Opciones;

/// <summary>
/// Sección <c>Notificaciones</c> (variables <c>Notificaciones__*</c> del .env): política de envío y reintentos del
/// correo de las notificaciones (HU-118, RN-78). Todos tienen valor por defecto; el .env solo los cambia.
/// </summary>
public sealed class NotificacionesOpciones
{
    public const string Seccion = "Notificaciones";

    /// <summary>false apaga el proceso en segundo plano (las pruebas lo ejecutan a mano).</summary>
    public bool Habilitado { get; set; } = true;

    /// <summary>Cada cuánto se buscan correos pendientes.</summary>
    public int IntervaloSegundos { get; set; } = 30;

    /// <summary>Intentos en total (el primero incluido). Al agotarlos, el correo queda "fallido".</summary>
    public int MaxIntentos { get; set; } = 5;

    /// <summary>Espera tras el primer fallo; se duplica en cada fallo siguiente (1, 2, 4, 8… minutos).</summary>
    public int EsperaBaseSegundos { get; set; } = 60;

    /// <summary>Tope de la espera entre intentos.</summary>
    public int EsperaMaximaSegundos { get; set; } = 3600;

    /// <summary>Correos por vuelta, para no acaparar la base ni el SMTP.</summary>
    public int LoteMaximo { get; set; } = 20;

    public bool SonValidas() =>
        IntervaloSegundos > 0 && MaxIntentos > 0 && EsperaBaseSegundos > 0
        && EsperaMaximaSegundos >= EsperaBaseSegundos && LoteMaximo > 0;

    /// <summary>Espera antes del próximo intento, después de <paramref name="intentosFallidos"/> fallos (≥ 1).</summary>
    public TimeSpan EsperaTras(int intentosFallidos)
    {
        var segundos = EsperaBaseSegundos * Math.Pow(2, Math.Min(intentosFallidos - 1, 20));
        return TimeSpan.FromSeconds(Math.Min(segundos, EsperaMaximaSegundos));
    }
}
