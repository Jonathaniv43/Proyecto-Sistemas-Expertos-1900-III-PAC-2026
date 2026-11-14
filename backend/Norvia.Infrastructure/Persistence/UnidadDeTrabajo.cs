using Norvia.Application.Common.Interfaces;

namespace Norvia.Infrastructure.Persistence;

/// <summary>Un SaveChanges de EF Core es una sola transacción con todos los cambios pendientes.</summary>
public sealed class UnidadDeTrabajo(NorviaDbContext db) : IUnidadDeTrabajo
{
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
