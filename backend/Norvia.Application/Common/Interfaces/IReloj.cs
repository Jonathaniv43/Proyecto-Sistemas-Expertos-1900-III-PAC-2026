namespace Norvia.Application.Common.Interfaces;

/// <summary>Hora oficial del sistema (la de PostgreSQL, <c>now()</c>), en UTC.</summary>
public interface IReloj
{
    Task<DateTime> AhoraAsync(CancellationToken cancellationToken = default);
}
