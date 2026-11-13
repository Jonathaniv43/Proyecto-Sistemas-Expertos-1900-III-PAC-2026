using Microsoft.EntityFrameworkCore;
using Norvia.Application.Features.Auth.Interfaces;
using Norvia.Infrastructure.Persistence;

namespace Norvia.Infrastructure.Auth;

public class RelojBaseDatos(NorviaDbContext db) : IRelojBaseDatos
{
    public async Task<DateTime> AhoraAsync(CancellationToken cancellationToken = default)
    {
        var conn = db.Database.GetDbConnection();
        var cerrar = conn.State != System.Data.ConnectionState.Open;
        if (cerrar)
            await conn.OpenAsync(cancellationToken);

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT now()";
            var valor = await cmd.ExecuteScalarAsync(cancellationToken);
            var dt = (DateTime)valor!;
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
