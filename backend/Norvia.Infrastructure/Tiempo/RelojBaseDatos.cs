using System.Data;
using Microsoft.EntityFrameworkCore;
using Norvia.Application.Common.Interfaces;
using Norvia.Infrastructure.Persistence;

namespace Norvia.Infrastructure.Tiempo;

/// <summary>Toma la hora de PostgreSQL (<c>now()</c>), no la del proceso de la API.</summary>
public sealed class RelojBaseDatos(NorviaDbContext db) : IReloj
{
    public async Task<DateTime> AhoraAsync(CancellationToken cancellationToken = default)
    {
        var conn = db.Database.GetDbConnection();
        var cerrar = conn.State != ConnectionState.Open;
        if (cerrar)
            await conn.OpenAsync(cancellationToken);

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT now()";
            var dt = (DateTime)(await cmd.ExecuteScalarAsync(cancellationToken))!;
            return dt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                : dt.ToUniversalTime();
        }
        finally
        {
            if (cerrar)
                await conn.CloseAsync();
        }
    }
}
