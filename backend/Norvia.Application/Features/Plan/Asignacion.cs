using Norvia.Domain.Entities.Acceso;

namespace Norvia.Application.Features.Plan;

/// <summary>Reglas compartidas al asignar una tarea (HU-043), sea del plan o manual.</summary>
public static class Asignacion
{
    /// <summary>
    /// Roles que pueden ser responsables de una tarea: los que trabajan con el plan. Los encuestados son cuentas solo
    /// para responder encuestas (HU-115) y no entran al plan.
    /// </summary>
    public static readonly short[] RolesResponsables = [Rol.AdminEmpresaId, Rol.ResponsableSstId, Rol.ColaboradorId];

    /// <summary>Honduras no tiene horario de verano: su fecha es la de UTC−6.</summary>
    public static DateOnly HoyEnHonduras(DateTime ahoraUtc) => DateOnly.FromDateTime(ahoraUtc.AddHours(-6));
}
