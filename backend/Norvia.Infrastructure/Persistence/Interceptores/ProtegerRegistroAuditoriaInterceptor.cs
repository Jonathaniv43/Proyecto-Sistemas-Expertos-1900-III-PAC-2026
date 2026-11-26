using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Norvia.Domain.Entities.Auditoria;

namespace Norvia.Infrastructure.Persistence.Interceptores;

/// <summary>
/// Garantiza la inalterabilidad de <see cref="RegistroAuditoria"/> en el uso normal del sistema (HU-099):
/// la aplicación solo inserta registros nuevos y rechaza cualquier modificación o eliminación.
/// </summary>
public sealed class ProtegerRegistroAuditoriaInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Validar(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Validar(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Validar(DbContext? context)
    {
        if (context is null)
            return;

        var alterada = context.ChangeTracker.Entries<RegistroAuditoria>()
            .Any(e => e.State is EntityState.Modified or EntityState.Deleted);

        if (alterada)
        {
            throw new InvalidOperationException(
                "RegistroAuditoria es inalterable: no se permite modificar ni eliminar evidencia registrada.");
        }
    }
}
