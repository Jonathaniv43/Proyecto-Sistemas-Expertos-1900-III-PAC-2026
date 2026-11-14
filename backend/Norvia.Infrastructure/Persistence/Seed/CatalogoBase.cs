using Norvia.Domain.Entities.Acceso;
using Norvia.Domain.Entities.Catalogos;

namespace Norvia.Infrastructure.Persistence.Seed;

/// <summary>
/// Datos fijos de HU-097 que van en las migraciones (HasData): roles de empresa, roles de staff y rubros.
/// Los Id son estables; no se cambian una vez publicados. El contenido SST (categorías, preguntas,
/// peligros y plantillas) se carga aparte cuando el equipo lo redacte y lo revise alguien de SST.
/// </summary>
public static class CatalogoBase
{
    /// <summary>Roles de empresa (VinculoEmpresa.RolId). Documentación: 05-actores-roles.md.</summary>
    public static readonly Rol[] Roles =
    [
        new() { Id = 1, Codigo = "admin_empresa", Nombre = "Administrador de empresa" },
        new() { Id = 2, Codigo = "responsable_sst", Nombre = "Responsable SST" },
        new() { Id = 3, Codigo = "colaborador", Nombre = "Colaborador" },
        new() { Id = 4, Codigo = "encuestado", Nombre = "Encuestado" },
    ];

    public const short RolStaffAdminId = 1;

    /// <summary>Roles del staff Norvia (StaffPlataforma.RolStaffPlataformaId, D23).</summary>
    public static readonly RolStaffPlataforma[] RolesStaff =
    [
        new() { Id = RolStaffAdminId, Codigo = "admin_plataforma", Nombre = "Administrador de plataforma" },
        new() { Id = 2, Codigo = "soporte_plataforma", Nombre = "Soporte de plataforma" },
    ];

    /// <summary>Los 4 rubros con que trabaja Norvia (RN-09).</summary>
    public static readonly Rubro[] Rubros =
    [
        new() { Id = 1, Codigo = "oficina", Nombre = "Oficina administrativa", Activo = true },
        new() { Id = 2, Codigo = "comercio", Nombre = "Comercio o tienda", Activo = true },
        new() { Id = 3, Codigo = "maquila_fabrica", Nombre = "Maquila o fábrica", Activo = true },
        new() { Id = 4, Codigo = "construccion", Nombre = "Construcción", Activo = true },
    ];
}
