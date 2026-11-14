using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Common;

namespace Norvia.Infrastructure.Persistence.Interceptores;

/// <summary>
/// Asigna <c>EmpresaId</c> desde la sesión al insertar filas de tablas de empresa y prohíbe
/// cambiarlo después (RN-04, RN-06). Lo que venga del cliente se sobrescribe.
/// Sin sesión de empresa (p. ej. el registro de una empresa nueva o un proceso de plataforma),
/// la aplicación debe fijar EmpresaId explícitamente; si no lo hace, se rechaza el guardado.
/// </summary>
public sealed class AsignarEmpresaInterceptor(ITenantActual tenant) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Aplicar(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Aplicar(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Aplicar(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entrada in context.ChangeTracker.Entries<ITieneEmpresa>())
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    if (tenant.EmpresaId is Guid empresa)
                        entrada.Entity.EmpresaId = empresa;
                    else if (entrada.Entity.EmpresaId == Guid.Empty)
                        throw new InvalidOperationException(
                            $"{entrada.Metadata.ClrType.Name}: no hay sesión de empresa y no se indicó EmpresaId.");
                    break;

                case EntityState.Modified when entrada.Property(e => e.EmpresaId).IsModified:
                    throw new InvalidOperationException(
                        $"{entrada.Metadata.ClrType.Name}: EmpresaId no se puede cambiar.");
            }
        }
    }
}
