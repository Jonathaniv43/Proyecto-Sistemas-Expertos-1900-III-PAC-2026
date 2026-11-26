using Norvia.Application.Common.Auditoria;
using Norvia.Domain.Entities.Auditoria;
using Norvia.Infrastructure.Data;

namespace Norvia.Infrastructure.Auditoria;

/// <summary>Solo inserta entradas nuevas en <see cref="RegistroAuditoria"/>; EmpresaId lo asigna el interceptor.</summary>
public sealed class RepositorioRegistroAuditoria(NorviaDbContext db) : IRegistroAuditoria
{
    public void Agregar(RegistroAuditoria registro) => db.RegistroAuditoria.Add(registro);
}
