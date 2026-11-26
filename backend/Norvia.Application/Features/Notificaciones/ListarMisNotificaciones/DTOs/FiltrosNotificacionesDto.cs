namespace Norvia.Application.Features.Notificaciones.ListarMisNotificaciones.DTOs;

/// <summary>Consulta de la bandeja (HU-117): <c>?limite=50</c>.</summary>
public sealed record FiltrosNotificacionesDto
{
    public const int LimitePorDefecto = 50;
    public const int LimiteMaximo = 100;

    /// <summary>Cuántas devolver, de la más reciente hacia atrás. Por defecto <see cref="LimitePorDefecto"/>.</summary>
    public int? Limite { get; init; }
}
