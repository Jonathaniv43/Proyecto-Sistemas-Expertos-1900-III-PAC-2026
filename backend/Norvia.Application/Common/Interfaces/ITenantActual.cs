using Norvia.Domain.Enums;

namespace Norvia.Application.Common.Interfaces;

/// <summary>
/// Empresa (tenant) de la sesión actual, tomada del JWT; nunca del cuerpo de la petición (RN-06).
/// Sin sesión de empresa (login, staff de plataforma, seeders) <see cref="EmpresaId"/> es nulo,
/// y las consultas a tablas de empresa no devuelven filas.
/// </summary>
public interface ITenantActual
{
    /// <summary>EmpresaId descifrado del claim <c>empresa_id</c>; nulo si no hay sesión de empresa.</summary>
    Guid? EmpresaId { get; }

    /// <summary>Vínculo autenticado (claim <c>vinculo_id</c>); nulo si no hay sesión de empresa.</summary>
    Guid? VinculoEmpresaId { get; }

    /// <summary>Tipo de sesión del token; nulo si la petición es anónima.</summary>
    TipoSesion? TipoSesion { get; }
}
