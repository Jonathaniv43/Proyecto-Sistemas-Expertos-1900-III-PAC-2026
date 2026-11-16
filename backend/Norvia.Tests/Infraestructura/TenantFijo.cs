using Norvia.Application.Common.Interfaces;
using Norvia.Domain.Enums;

namespace Norvia.Tests.Infraestructura;

/// <summary>Sesión fija para usar el DbContext fuera de una petición HTTP.</summary>
public sealed class TenantFijo(Guid? empresaId) : ITenantActual
{
    public Guid? EmpresaId => empresaId;
    public Guid? VinculoEmpresaId => null;
    public Guid? AccesoId => null;
    public TipoSesion? TipoSesion => empresaId is null ? null : Domain.Enums.TipoSesion.Empresa;
}
