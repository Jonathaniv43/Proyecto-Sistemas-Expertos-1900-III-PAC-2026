namespace Norvia.Application.Common.Interfaces;

/// <summary>
/// Confirma en una sola transacción los cambios que los repositorios dejaron pendientes.
/// Los repositorios nunca guardan; el servicio del caso de uso llama a este método una vez.
/// </summary>
public interface IUnidadDeTrabajo
{
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
