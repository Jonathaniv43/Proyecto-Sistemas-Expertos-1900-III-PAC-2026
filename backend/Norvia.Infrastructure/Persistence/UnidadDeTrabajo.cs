using Norvia.Application.Common.Interfaces;

using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Persistence;

/// <summary>
/// Un SaveChanges de EF Core es una sola transacción con todos los cambios pendientes; dentro de
/// <see cref="EnTransaccionAsync"/>, cada SaveChanges se suma a la transacción abierta.
/// </summary>
public sealed class UnidadDeTrabajo(NorviaDbContext db) : IUnidadDeTrabajo
{
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task EnTransaccionAsync(Func<CancellationToken, Task> trabajo, CancellationToken cancellationToken = default)
    {
        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);
        await trabajo(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);
    }
}
