namespace Norvia.Application.Features.Auth.Interfaces;

/// <summary>Hora de PostgreSQL (now()), no la del proceso de la API.</summary>
public interface IRelojBaseDatos
{
    Task<DateTime> AhoraAsync(CancellationToken cancellationToken = default);
}
