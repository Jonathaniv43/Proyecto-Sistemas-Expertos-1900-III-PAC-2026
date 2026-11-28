namespace Norvia.Application.Common.Notificaciones;

/// <summary>
/// Avisa al proceso de fondo que hay correos de notificaciones por enviar, para que no espere a su próxima
/// vuelta (RN-78). La API nunca envía el correo dentro de la petición: solo el proceso de fondo lo hace, así
/// ningún otro camino compite por la misma fila y cerrar la página no corta un envío a medias.
/// </summary>
public interface IAvisoCorreosPendientes
{
    /// <summary>Llamar después de guardar las notificaciones. No espera ni lanza.</summary>
    void Avisar();
}
