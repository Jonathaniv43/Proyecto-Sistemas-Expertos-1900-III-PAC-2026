using Norvia.Application.Common.Resultados;

namespace Norvia.Application.Features.Notificaciones;

/// <summary>Errores de negocio del módulo Notificaciones (M11), compartidos solo por sus slices.</summary>
public static class Errores
{
    public static readonly Error SinVinculo = new("notificaciones.sin_vinculo",
        "Las notificaciones son de una empresa: inicie sesión con su vínculo.", TipoError.Prohibido);

    /// <summary>RN-79: la de otra persona responde igual que una inexistente, para no confirmar que existe.</summary>
    public static readonly Error NotificacionNoEncontrada = new("notificaciones.no_encontrada",
        "La notificación no existe.", TipoError.NoEncontrado);
}
