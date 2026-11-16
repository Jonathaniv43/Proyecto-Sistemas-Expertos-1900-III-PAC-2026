using Norvia.Application.Common.Interfaces;
using Enums = Norvia.Domain.Enums;

namespace Norvia.Tests.Infraestructura;

/// <summary>Sesión fija para usar el DbContext fuera de una petición HTTP.</summary>
public sealed class TenantFijo(Guid? empresaId) : ITenantActual
{
    public Guid? EmpresaId => empresaId;
    public Guid? VinculoEmpresaId => null;
    public Guid? AccesoId => null;
    public Enums.TipoSesion? TipoSesion => empresaId is null ? null : Enums.TipoSesion.Empresa;
}
