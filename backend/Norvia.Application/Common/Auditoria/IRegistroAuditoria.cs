using Norvia.Domain.Entities.Auditoria;

namespace Norvia.Application.Common.Auditoria;

public interface IRegistroAuditoria
{
    void Agregar(RegistroAuditoria registro);
}
