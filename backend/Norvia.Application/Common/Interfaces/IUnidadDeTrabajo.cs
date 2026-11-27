namespace Norvia.Application.Common.Interfaces;

/// <summary>
/// Confirma en una sola transacción los cambios que los repositorios dejaron pendientes.
/// Los repositorios nunca guardan; el servicio del caso de uso llama a este método una vez.
/// </summary>
public interface IUnidadDeTrabajo
{
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Para cuando la base debe ver los cambios en un orden fijo (por ejemplo, liberar un valor único antes de que otra
    /// fila lo tome): dentro de <paramref name="trabajo"/> se llama a <see cref="GuardarCambiosAsync"/> en cada paso y
    /// todos se confirman juntos al terminar. Si algo falla, no queda ninguno.
    /// </summary>
    Task EnTransaccionAsync(Func<CancellationToken, Task> trabajo, CancellationToken cancellationToken = default);
}
